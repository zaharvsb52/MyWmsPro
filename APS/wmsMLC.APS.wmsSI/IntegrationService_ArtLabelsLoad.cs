using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.DAL;
using System.Threading.Tasks;
using wmsMLC.APS.wmsSI.Helpers;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        public ErrorWrapper[] LabelsLoad(LabelWrapper[] labelPackage)
        {
            Contract.Requires(labelPackage != null);

            Log.InfoFormat("Start of LabelsLoad");
            Log.Debug(labelPackage.DumpToXML());
            var startAllTime = DateTime.Now;
            //Log.Debug(labelPackage.DumpToXML());
            var retMessage = new List<ErrorWrapper>();

            try
            {
                // проверяем наличие артикулов
                FillArtLabels(labelPackage, retMessage);

                // проверяем наличие этикеток
                FillCodeLabels(labelPackage, retMessage);

                //var processorCount = Environment.ProcessorCount;
                var parallelOptions = new ParallelOptions
                {
                    //MaxDegreeOfParallelism = (processorCount <= 1 ? 8 : processorCount)
                    MaxDegreeOfParallelism = 20
                };
                Parallel.ForEach(labelPackage, parallelOptions, item =>
                {
                    IUnitOfWork uow = null;
                    try
                    {
                        uow = UnitOfWorkHelper.GetUnit();

                        var startTime = DateTime.Now;
                        Log.DebugFormat("Загружаем этикетку «{0}» артикула «{1}» манданта «{2}»", item.LABELCODE, item.ARTNAME, item.MandantCode);

                        uow.BeginChanges();

                        LabelLoadInternal(item, retMessage, uow);

                        Log.DebugFormat("Этикетка «{0}» артикула «{1}» загружена за {2}", item.LABELCODE, item.ARTNAME, DateTime.Now - startTime);
                        uow.CommitChanges();
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.SuccessCode.ToString(),
                            ERRORMESSAGE = string.Format("Загружена этикетка «{0}» артикула «{1}»", item.LABELCODE, item.ARTNAME)
                        };
                        retMessage.Add(ew);
                    }
                    catch (IntegrationLogicalException iex)
                    {
                        if (uow != null)
                            uow.RollbackChanges();

                        var message = ExceptionHelper.ExceptionToString(iex);
                        Log.Error(message, iex);

                        var ew = new ErrorWrapper { ERRORCODE = MessageHelper.ErrorCode.ToString(), ERRORMESSAGE = message };
                        retMessage.Add(ew);
                    }
                    catch (Exception ex)
                    {
                        if (uow != null)
                            uow.RollbackChanges();

                        var message = ExceptionHelper.ExceptionToString(ex);
                        Log.Error(message, ex);

                        var ew = new ErrorWrapper { ERRORCODE = MessageHelper.ErrorCode.ToString(), ERRORMESSAGE = "Системная ошибка: " + message };
                        retMessage.Add(ew);
                    }
                    finally
                    {
                        if (uow != null)
                            uow.Dispose();
                    }
                });
            }
            catch (IntegrationLogicalException iex)
            {
                var message = ExceptionHelper.ExceptionToString(iex);
                Log.Error(message, iex);

                var ew = new ErrorWrapper { ERRORCODE = MessageHelper.ErrorCode.ToString(), ERRORMESSAGE = message };
                retMessage.Add(ew);
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);
                Log.Error(message, ex);
                throw new FaultException<string>(message, new FaultReason(message));
            }
            finally
            {
                Log.DebugFormat("Общее время загрузки {0}", DateTime.Now - startAllTime);
                Log.InfoFormat("End of LabelsLoad");
            }
            return retMessage.ToArray();
        }

        private void FillArtLabels(LabelWrapper[] labelPackage, List<ErrorWrapper> retMessage)
        {
            var artNames = labelPackage.Where(p => !string.IsNullOrEmpty(p.ARTNAME)).Select(p => p.ARTNAME.ToUpper()).Distinct().ToArray();
            if (artNames.Length == 0)
            {
                var message = "Загрузка стикеров. Отсутствуют артикула.";
                var ew = new ErrorWrapper
                {
                    ERRORCODE = MessageHelper.ErrorCode.ToString(),
                    ERRORMESSAGE = message
                };
                retMessage.Add(ew);
                Log.Debug(message);
                return;
            }

            var arts = new List<Art>();
            var typeart = typeof (Art);
            var artsList =
                FilterHelper.GetArrayFilterIn(
                    string.Format("UPPER({0})",
                        SourceNameHelper.Instance.GetPropertySourceName(typeart, Art.ArtNamePropertyName)), artNames,
                    string.Format(" and {0} = {1}",
                        SourceNameHelper.Instance.GetPropertySourceName(typeart,
                            Art.MANDANTIDPropertyName), labelPackage[0].MANDANTID));

            using (var mgrArt = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                foreach (var artFilter in artsList)
                {
                    var artspart = mgrArt.GetFiltered(artFilter, GetModeEnum.Partial).ToArray();
                    if (artspart.Length > 0)
                        arts.AddRange(artspart);
                }

                var nonexistentArt = new List<LabelWrapper>();
                foreach (var label in labelPackage)
                {
                    var existsArt = arts.Where(i => label.ARTNAME.EqIgnoreCase(i.ArtName)).ToArray();

                    if (existsArt.Length > 1 && (existsArt = arts.Where(i => label.ARTNAME == i.ArtName).ToArray()).Length > 1)
                    {
                        var message = string.Format("Существует несколько значений для артикула «{0}»", label.ARTNAME);
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = message
                                
                        };
                        retMessage.Add(ew);
                        Log.Debug(message);
                        nonexistentArt.Add(label);
                        continue;
                    }

                    if (existsArt.Length == 0)
                    {
                        var message = string.Format("Не найден артикул «{0}»", label.ARTNAME);
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = message
                        };
                        Log.Debug(message);
                        retMessage.Add(ew);
                        nonexistentArt.Add(label);
                        continue;
                    }
                   
                    label.ARTCODE = existsArt[0].ArtCode;
                }

                if (nonexistentArt.Count > 0)
                    labelPackage = labelPackage.Where(i => nonexistentArt.All(x => x.ARTNAME != i.ARTNAME)).ToArray();

                //if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                //    throw new IntegrationLogicalException("Ошибка при проверке артикулов (этикетки)");
            }
        }

        private void FillCodeLabels(LabelWrapper[] labelPackage, List<ErrorWrapper> retMessage)
        {
            var errCountOld = MessageHelper.GetErrorCount(retMessage);
            var codeLabels = labelPackage.Select(i => i.LABELCODE).Distinct();
            var codeLabelsFilterValue = "'" + string.Join("','", codeLabels) + "'";
            var codeLabelsFilter = string.Format("{0} in ({1}) and {2} = {3}",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Label), Label.LabelCodePropertyName), codeLabelsFilterValue,
                    SourceNameHelper.Instance.GetPropertySourceName(typeof(Label), Label.MANDANTIDPropertyName), labelPackage[0].MANDANTID);

            using (var mgrLabel = IoC.Instance.Resolve<IBaseManager<Label>>())
            {
                var labels = mgrLabel.GetFiltered(codeLabelsFilter).ToArray();

                foreach (var labelWr in labelPackage)
                {
                    var existsLabel = labels.Where(i => labelWr.LABELCODE.EqIgnoreCase(i.LabelCode)).ToArray();
                    if (existsLabel.Length == 0)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не существует этикетка «{0}» для артикула «{1}» манданта «{2}»",
                            labelWr.LABELCODE, labelWr.ARTNAME, labelWr.MandantCode)
                        };
                        retMessage.Add(ew);
                    }
                    else
                    {
                        var labelUseFilter = string.Format("{0} = '{1}' and {2} = '{3}'",
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelUse),
                                LabelUse.ArtCode_RPropertyName), labelWr.ARTCODE,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelUse),
                                LabelUse.LabelCode_RPropertyName), labelWr.LABELCODE);

                        using (var mgrLabelUse = IoC.Instance.Resolve<IBaseManager<LabelUse>>())
                        {
                            var labelUses = mgrLabelUse.GetFiltered(labelUseFilter).ToArray();
                            var lparams = labelWr.LabelParamsL.Where(i => !i.LABELUSEID_R.HasValue);
                            if (labelUses.Length == 0)
                            {
                                var newLabelUse = new LabelUse
                                {
                                    LabelCode_R = labelWr.LABELCODE,
                                    ArtCode_R = labelWr.ARTCODE
                                };
                                SetXmlIgnore(newLabelUse, false);
                                mgrLabelUse.Insert(ref newLabelUse);
                                Log.DebugFormat("Создано применение этикетки «{0}» для артикула «{1}»", labelWr.LABELCODE, labelWr.ARTNAME);
                                foreach (var lparam in lparams)
                                    lparam.LABELUSEID_R = newLabelUse.LabelUseID;
                            }
                            else
                            {
                                Log.DebugFormat("Существует применение этикетки «{0}» для артикула «{1}»", labelWr.LABELCODE, labelWr.ARTNAME);
                                foreach (var lparam in lparams)
                                    lparam.LABELUSEID_R = labelUses[0].LabelUseID;
                            }
                        }
                    }
                }

                if (MessageHelper.GetErrorCount(retMessage) > errCountOld)
                    throw new IntegrationLogicalException("Ошибка при проверке этикеток");
            }
        }

        private void LabelLoadInternal(LabelWrapper item, List<ErrorWrapper> retMessage, IUnitOfWork uow)
        {
            using (var lpMgr = IoC.Instance.Resolve<IBaseManager<LabelParams>>())
            {
                if (uow != null)
                    lpMgr.SetUnitOfWork(uow);

                var lparNames = item.LabelParamsL.Select(i => i.LABELPARAMSNAME).Distinct();

                var lpFilterList = FilterHelper.GetArrayFilterIn(SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelParams),
                                                         LabelParams.LabelParamsNamePropertyName), lparNames,
                                                         string.Format(" and {0} = '{1}'",
                                                         SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelParams),
                                                         LabelParams.LabelCodeRPropertyName), item.LABELCODE));
                var labelparams = new List<LabelParams>();
                foreach (var lpFilter in lpFilterList)
                {
                    labelparams.AddRange(lpMgr.GetFiltered(lpFilter).ToArray());
                }
                foreach (var lpWrapper in item.LabelParamsL)
                {
                    var existsLabelParams = labelparams.Where(i => lpWrapper.LABELPARAMSNAME.EqIgnoreCase(i.LabelParamsName)).ToArray();
                    if (existsLabelParams.Length == 0)
                    {
                        var ew = new ErrorWrapper
                        {
                            ERRORCODE = MessageHelper.ErrorCode.ToString(),
                            ERRORMESSAGE = string.Format("Не существует параметр «{0}» для этикетки «{1}» артикула «{2}»",
                            lpWrapper.LABELPARAMSNAME, item.LABELCODE, item.ARTNAME)
                        };
                        retMessage.Add(ew);
                    }
                    else
                    {
                        var lpvMgr = IoC.Instance.Resolve<IBaseManager<LabelParamsValue>>();
                        var lpvFilter = string.Format("{0} = {1} and {2} = {3}",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelParamsValue), LabelParamsValue.LabelUseIDRPropertyName), lpWrapper.LABELUSEID_R,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(LabelParamsValue), LabelParamsValue.LabelParamsIDRPropertyName), existsLabelParams[0].LabelParamsID);
                        var labelParamsValue = lpvMgr.GetFiltered(lpvFilter).ToArray();
                        if (labelParamsValue.Length == 0)
                        {
                            var newLabelParamsValue = new LabelParamsValue
                            {
                                LabelUseIDR = lpWrapper.LABELUSEID_R,
                                LabelParamsIDR = existsLabelParams[0].LabelParamsID,
                                LabelParamsText = lpWrapper.LABELPARAMSVALUE
                            };
                            SetXmlIgnore(newLabelParamsValue, false);
                            lpvMgr.Insert(ref newLabelParamsValue);
                        }
                        else
                        {
                            var firstLabelParamsValue = labelParamsValue[0];
                            firstLabelParamsValue.LabelParamsText = lpWrapper.LABELPARAMSVALUE;

                            if (firstLabelParamsValue.IsDirty)
                            {
                                SetXmlIgnore(firstLabelParamsValue, false);
                                lpvMgr.Update(firstLabelParamsValue);
                                Log.DebugFormat("Обновлен параметр «{0}» для этикетки «{1}» артикула «{2}»",
                                    lpWrapper.LABELPARAMSNAME, item.LABELCODE, item.ARTNAME);
                            }
                            else
                            {
                                Log.DebugFormat("Обновление параметра «{0}» для этикетки «{1}» артикула «{2}» не требуется",
                                    lpWrapper.LABELPARAMSNAME, item.LABELCODE, item.ARTNAME);
                            }
                        }
                    }
                }
                if (retMessage.Any(mess => mess.ERRORCODE.Equals(MessageHelper.ErrorCode.ToString())))
                    throw new IntegrationLogicalException("Ошибка при проверке параметров этикеток");
            }
        }
    }
}
