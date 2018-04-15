using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI.Helpers
{
    public static class CpvHelper
    {
        public static decimal GetMinCpvId(decimal?[] keys)
        {
            const decimal minCpvId = -100;
            if (keys == null || keys.Length == 0)
                return minCpvId;

            var ids = keys.Where(p => p.HasValue).Select(p => p.Value).ToArray();
            if (ids.Length == 0)
                return minCpvId;
            return ids.Min();
        }
    }

    public sealed class CpvHelper<T> where T : CustomParamValue
    {
        private int _cpvid;
        private readonly string _cpEntity;
        private readonly string _cpvKey;

        public CpvHelper(string cpEntity, string cpvKey)
        {
            if (string.IsNullOrEmpty(cpEntity))
                throw new ArgumentNullException("cpEntity");

            _cpEntity = cpEntity;
            _cpvKey = cpvKey;
        }

        /// <summary>
        /// Получить все CPV.
        /// </summary>
        public T[] GetAllCpv(IUnitOfWork uow)
        {
            List<T> wmscpvs;

            //Получаем список cpv
            var cpvtype = typeof(T);
            var filter = string.Format("{0} = '{1}' AND {2} = '{3}'",
                SourceNameHelper.Instance.GetPropertySourceName(cpvtype, CustomParamValue.CPV2EntityPropertyName),
                _cpEntity,
                SourceNameHelper.Instance.GetPropertySourceName(cpvtype, CustomParamValue.CPVKeyPropertyName),
                _cpvKey);
            using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                wmscpvs = mgr.GetFiltered(filter, GetModeEnum.Partial).ToList();
            }

            //Проверяем CPVParent на неотрицательные значения
            foreach (var cpv in wmscpvs.Where(cpv => cpv.CPVParent <= 0))
            {
                cpv.CPVParent = null;
            }

            var cps = GetAllCp(uow);

            // получим корневые элементы cp - они обязаны быть
            var rootElements = cps.Where(i => string.IsNullOrEmpty(i.CustomParamParent)).ToArray();
            if (rootElements.Length == 0)
                throw new DeveloperException("Отсутствуют корневые элементы CPV для сущности '{0}'.", _cpEntity);

            // добавим корневые cpv, которые отсутствуют
            foreach (var root in rootElements)
            {
                var exists = wmscpvs.FirstOrDefault(i => i.CustomParamCode.EqIgnoreCase(root.GetKey<string>()));
                if (exists == null)
                    wmscpvs.Add(CreateItem(cp: root, parentCpvId: null));
            }

            var rootCpvs = wmscpvs.Where(i => i.CPVParent == null).ToArray();
            
            // проходим по всем корневым cpv и добираем отсутствущие элементы
            foreach (var cpv in rootCpvs)
            {
                CollectItems(cpv, cps.ToArray(), wmscpvs);
            }

            foreach (var cpv in wmscpvs)
            {
                cpv.Cp = cps.FirstOrDefault(i => i.GetKey<string>() == cpv.CustomParamCode);
                cpv.AcceptChanges();
            }

            return wmscpvs.ToArray();
        }

        /// <summary>
        /// Получить список параметров с учетом манданта.
        /// </summary>
        public CustomParam[] GetCpByMandant(string cpSource, string cpTarget, IUnitOfWork uow)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<CustomParam>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                return ((ICustomParamManager)mgr).GetCPByInstance(_cpEntity, _cpvKey, null, cpSource, cpTarget).ToArray();
            }
        }

        /// <summary>
        /// Получить список параметров без учета манданта.
        /// </summary>
        public CustomParam[] GetAllCp(IUnitOfWork uow)
        {
            var filter = string.Format("{0} = '{1}'",
                SourceNameHelper.Instance.GetPropertySourceName(typeof (CustomParam),
                    CustomParam.CustomParam2EntityPropertyName), _cpEntity);
            using (var mgr = IoC.Instance.Resolve<IBaseManager<CustomParam>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                return mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            }
        }

        private void CollectItems(T cpv, CustomParam[] cps, List<T> items)
        {
            var childs = cps.Where(i => i.CustomParamParent.EqIgnoreCase(cpv.CustomParamCode)).ToArray();
            if (childs.Length > 0)
            {
                foreach (var cp in childs)
                {
                    // есть ли уже этот элемент в коллекции
                    var existsChild =
                        items.FirstOrDefault(
                            i => i.CPVParent == cpv.CPVID && i.CustomParamCode == cp.GetKey<string>());
                    if (existsChild == null)
                    {
                        existsChild = CreateItem(cp, cpv.CPVID);
                        items.Add(existsChild);
                    }
                    existsChild.Cp = cp;
                    // пусть соберет своих деток
                    CollectItems(existsChild, cps, items);
                }
            }
        }

        private T CreateItem(CustomParam cp, decimal? parentCpvId)
        {
            var cpv = (T) Activator.CreateInstance(typeof (T));
            cpv.CPVID = --_cpvid;
            cpv.CustomParamCode = cp.GetKey<string>();
            cpv.CPV2Entity = _cpEntity;
            cpv.CPVKey = _cpvKey;
            cpv.CPVValue = cp.CustomParamDefault;
            cpv.VCustomParamParent = cp.CustomParamParent;
            cpv.Cp = cp;
            cpv.CPVParent = parentCpvId;
            cpv.VCustomParamCount = cp.CustomParamCount;
            cpv.VCustomParamDesc = cp.CustomParamDesc;
            return cpv;
        }

        public void Save(T[] source, bool allowUpdate, bool includeCpvWithDafaultValue, bool verify, IUnitOfWork uow)
        {
            var changes = GetChanges(source, includeCpvWithDafaultValue);
            if (changes == null || !changes.Any())
                return;

            if (verify)
            {
                var relatedChanges = CollectRelatedParams(source, changes);
                if (!Validation(relatedChanges))
                    return;
            }

            var sources = source.OrderByDescending(cpv => cpv.CPVParent, new SpecialComparer()).ToArray();
            using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                T currentcpv = null;
                try
                {
                    foreach (var cpv in sources)
                    {
                        currentcpv = cpv;
                        var key = cpv.GetKey<decimal>();
                        if (key < 0)
                        {
                            var item = cpv;

                            if ((includeCpvWithDafaultValue && !string.IsNullOrEmpty(cpv.CPVValue)) || //значения по умолчанию
                                (!includeCpvWithDafaultValue && HasChanges(GetChanges(new[] { cpv }, false))) ||
                                //cpv.Cp.CUSTOMPARAMSAVEMODE ||
                                HasChanges(GetChanges(GetChildsCpvByParentCpv(sources, cpv, false), includeCpvWithDafaultValue)))
                            {
                                if (item.CPVParent <= 0)
                                {
                                    item.CPVParent = null;
                                    cpv.CPVParent = null;
                                }
                                mgr.Insert(ref item);

                                item.Cp = cpv.Cp;
                                var childs = sources.Where(i => i.CPVParent == key);
                                foreach (var child in childs)
                                {
                                    var isdirty = HasChanges(GetChanges(new[] { child }, false));
                                    child.CPVParent = item.CPVID;
                                    if (!isdirty)
                                        child.AcceptChanges();
                                }
                            }
                            else
                            {
                                cpv.AcceptChanges();
                            }
                        }
                        else
                        {
                            if (allowUpdate)
                            {
                                //Удаляем параметры, у которых не установлен CUSTOMPARAMSAVEMODE и CPVValue is null и нет деток
                                if (string.IsNullOrEmpty(cpv.CPVValue) && !cpv.Cp.CUSTOMPARAMSAVEMODE &&
                                    GetChildsCpvByParentCpv(sources, cpv, false).Length == 0)
                                {
                                    mgr.Delete(cpv);
                                    cpv.AcceptChanges();
                                }
                                else if (cpv.IsDirty)
                                {
                                    if (cpv.CPVParent <= 0)
                                        cpv.CPVParent = null;
                                    mgr.Update(cpv);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (currentcpv != null)
                        throw new OperationException(
                            string.Format("Ошибка при сохранении cpv '{0}' (ид. '{1}').", currentcpv.CustomParamCode, currentcpv.GetKey()), ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Удаление ссылок на несуществкющих родителей для новых элементов.
        /// </summary>
        public void ClearBadParent(ICollection<T> cpvs)
        {
            foreach (var cpv in cpvs.Where(p => p.CPVParent <= 0 && cpvs.All(i => i.CPVID != p.CPVParent)))
            {
                cpv.CPVParent = null;
            }
        }

        private bool Validation(ICollection<T> cpvs)
        {
            if (cpvs == null || cpvs.Count == 0)
                return false;

            var mustsetcpvs = cpvs.Where(cpv => cpv.Cp.CustomParamMustSet).ToArray();
            // INFO: мы уже собрали все необходимые CustomParamMustSet, проверяем на наличие значения
            //INFO: не собрали, проход по верхнему уровню cpv, без учета mustset.
            var badcpv = mustsetcpvs.Where(cpv => !cpv.IsReadOnly && !cpv.Cp.CustomparamInputdisable && string.IsNullOrEmpty(cpv.CPVValue)).ToArray();
            
            if (badcpv.Any())
            {
                var message = string.Format("Не заполнено значение у параметров:{0}{1}",
                    Environment.NewLine,
                    string.Join(Environment.NewLine, badcpv.Select(p => string.Format("'{0}' ('{1}', ид. '{2}')", p.Cp.CustomParamName, p.CustomParamCode, p.GetKey()))));
                throw new OperationException(message);
                //return false;
            }

            return true;
        }

        private T[] GetChanges(IEnumerable<T> source, bool includeCpvWithDafaultValue)
        {
            if (source == null)
                return new T[0];

            if (includeCpvWithDafaultValue)
            {
                return
                    source.Where(
                        p =>
                            !string.IsNullOrEmpty(p.CPVValue) ||
                            //Если были проставлены значения по умолчанию - сохраняем
                            p.IsNew || p.IsDirty || p.Cp.CUSTOMPARAMSAVEMODE).ToArray();
            }
            return source.Where(p => p.IsNew || p.IsDirty).ToArray();
        }

        private T[] CollectRelatedParams(IEnumerable<T> source, IEnumerable<T> dirties)
        {
            var result = new List<T>();
            if (source == null)
                return dirties == null ? new T[0] : dirties.ToArray();
            if (dirties == null)
                return new T[0];
            var dirtiesArray = dirties as T[] ?? dirties.ToArray();
            result.AddRange(dirtiesArray);
            var sourceArray = source as T[] ?? source.ToArray();
            foreach (var dirty in dirtiesArray)
            {
                // соберем на нижнем уровне
                var lowerLevelItems = sourceArray.Where(i => i.CPVParent == dirty.CPVID && i.Cp.CustomParamMustSet).ToArray();
                if (lowerLevelItems.Any())
                    result.AddRange(lowerLevelItems);
                // соберем на одном уровне
                var sameLevelItems = sourceArray.Where(i => i.CPVID != dirty.CPVID && i.CPVParent == dirty.CPVParent && i.Cp.CustomParamMustSet).ToArray();
                if (sameLevelItems.Any())
                    result.AddRange(sameLevelItems);
                // соберем на верхнем уровне
                var upperLevelItems = sourceArray.Where(i => i.CPVID == dirty.CPVParent).ToArray();
                if (upperLevelItems.Any())
                {
                    // идем вверх по дереву
                    var others = CollectRelatedParams(sourceArray, upperLevelItems);
                    if (others.Any())
                    {
                        foreach (var other in others)
                            if (result.FirstOrDefault(i => i.CPVID == other.CPVID) == null)
                                result.Add(other);
                    }
                }
            }
            return result.Distinct(new CpvComparer<T>()).ToArray();
        }

        private T[] GetChildsCpvByParentCpv(T[] cpvs, T parentcpv, bool addToResultParentcpv)
        {
            if (parentcpv == null || cpvs == null)
                return new T[0];

            var result = new List<T>();
            if (addToResultParentcpv)
                result.Add(parentcpv);

            foreach (var p in cpvs.Where(p => p.CPVParent == parentcpv.CPVID))
            {
                result.Add(p);
                var childs = GetChildsCpvByParentCpv(cpvs, p, false);
                if (childs.Length > 0)
                    result.AddRange(childs);
            }
            return result.ToArray();
        }

        private bool HasChanges(IEnumerable<CustomParamValue> changes)
        {
            return changes != null && changes.Any();
        }

        private class CpvComparer<TCpv> : IEqualityComparer<TCpv> where TCpv : CustomParamValue
        {
            public bool Equals(TCpv x, TCpv y)
            {
                //Check whether the compared objects reference the same data.
                if (ReferenceEquals(x, y))
                    return true;

                //Check whether any of the compared objects is null.
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                    return false;

                return Equals(x.GetKey(), y.GetKey());
            }

            public int GetHashCode(TCpv obj)
            {
                return obj == null ? 0 : obj.GetKey().GetHashCode();
            }
        }
    }

    internal class SpecialComparer : IComparer<decimal?>
    {
        public int Compare(decimal? x, decimal? y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return 1;
            if (y == null)
                return -1;
            if (x.Value == y.Value)
                return 0;
            return x.Value > y.Value ? 1 : -1;
        }
    }
}
