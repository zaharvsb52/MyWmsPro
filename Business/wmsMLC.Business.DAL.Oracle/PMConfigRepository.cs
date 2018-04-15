using System.Collections.Generic;
using System.Linq;
using System.Xml;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class PMConfigRepository : BaseHistoryRepository<PMConfig, decimal>, IPMConfigRepository
    {
        private const string GetPMConfigByParamListFunctionName = "pkgPMConfig.getPMConfigByParamLst";

        public List<PMConfig> GetPMConfigByParamListByArtCode(string artCode, string operationCode, string methodName)
        {
            return GetPMConfigByParamList(null, new [] {artCode}, operationCode, methodName);
        }

        public List<PMConfig> GetPMConfigByParamListByProductId(decimal? productId, string operationCode, string methodName)
        {
            return GetPMConfigByParamList(new [] {productId}, null, operationCode, methodName);
        }

        private List<PMConfig> GetPMConfigByParamList(decimal?[] productIdList, string[] artCodeList, string operationCode, string methodName)
        {
            if ((productIdList == null || productIdList.Length == 0) && (artCodeList == null || artCodeList.Length == 0))
                throw new DeveloperException("Parameters productIdList[] and artCodeList[] can't be empty at the same time");

            if ((productIdList != null && productIdList.Length > 0) && (artCodeList != null && artCodeList.Length > 0))
                throw new DeveloperException("Parameters productIdList[] and artCodeList[] can't be not empty at the same time");

            var byProduct = productIdList != null && productIdList.Length > 0;
            var count = byProduct ? productIdList.Length : artCodeList.Length;

            var result = new List<PMConfig>();
            //function getPMConfigByParamLst (pProductID in INTEGER,pArtCode in VARCHAR2,pOperationCode in VARCHAR2,pMMethodCode in VARCHAR2,pAttrEntity in XMLType default NULL,pFilter in VARCHAR2 default NULL) return TListXml pipelined as
            return RunManualDbOperation(db =>
            {
                var ps = db.GetSpParameters(GetPMConfigByParamListFunctionName, false, false);

                for (int i = 0; i < count; i++)
                {
                    ps[0].Value = byProduct ? productIdList[i] : null;
                    ps[1].Value = byProduct ? null : artCodeList[i];
                    ps[2].Value = operationCode;
                    ps[3].Value = methodName;

                    var stm = string.Format("select SYS.XMLTYPE.GETCLOBVAL(COLUMN_VALUE) from TABLE({0}(:pproductid, :partcode, :poperationcode, :pmmethodcode, :pattrentity, :pfilter))",
                        GetPMConfigByParamListFunctionName);
                    var resXml = db.SetCommand(stm, ps).ExecuteScalarList<XmlDocument>();

                    result.AddRange(XmlDocumentConverter.ConvertToListOf<PMConfig>(resXml));
                }
                return result;
            });
        }
    }
}
