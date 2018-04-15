using System;
using System.Collections.Generic;
using wmsMLC.General.Resources;

namespace wmsMLC.General.BL
{
    /// <summary>
    /// Менеждер безопасности
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    /// <typeparam name="TKey">Тип ключа сущности</typeparam>
    public class SecurityManager<T, TKey> : BusinessObjectManager<T, TKey>, ISecurityAccess where T : class
    {
        public const string CreateRightName = "C";
        public const string ReadRightName   = "R";
        public const string UpdateRightName = "U";
        public const string DeleteRightName = "D";

        #region . Fields .
        private static volatile Lazy<ISecurityChecker> SecurityChecker =
            new Lazy<ISecurityChecker>(() =>
                {
                    try
                    {
                        return IoC.Instance.Resolve<ISecurityChecker>();
                    }
                    catch
                    {
                        return null;
                    }
                }
                );

        private bool _skipRightChecking;
        #endregion

        #region . IBaseManager .
        public override T New()
        {
            Check(CreateRightName);
            return base.New();
        }

        public override void Insert(ref T entity)
        {
            Check(CreateRightName);
            base.Insert(ref entity);
        }

        public override void Insert(ref IEnumerable<T> entities)
        {
            Check(CreateRightName);
            base.Insert(ref entities);
        }

        public override void Update(T entity)
        {
            Check(UpdateRightName);
            base.Update(entity);
        }

        public override void Update(IEnumerable<T> entities)
        {
            Check(UpdateRightName);
            base.Update(entities);
        }

        public override void Delete(T entity)
        {
            Check(DeleteRightName);
            base.Delete(entity);
        }

        public override void Delete(IEnumerable<T> entities)
        {
            Check(DeleteRightName);
            base.Delete(entities);
        }

        public override T Get(TKey key, GetModeEnum mode = GetModeEnum.Full)
        {
            Check(ReadRightName);
            return base.Get(key, mode);
        }

        public override IEnumerable<T> GetAll(GetModeEnum mode = GetModeEnum.Full)
        {
            Check(ReadRightName);
            return base.GetAll(mode);
        }

        public override IEnumerable<T> GetFiltered(string filter, GetModeEnum mode = GetModeEnum.Full)
        {
            Check(ReadRightName);
            return base.GetFiltered(filter, mode);
        }

        public override void Load(T entity)
        {
            Check(ReadRightName);
            base.Load(entity);
        }

        public override void ClearCache()
        {
            // права не проверяем
            base.ClearCache();
        }
        #endregion

        #region . Methods .

        void ISecurityAccess.SuspendRightChecking()
        {
            _skipRightChecking = true;
        }

        void ISecurityAccess.ResumeRightChecking()
        {
            _skipRightChecking = false;
        }

        protected virtual void Check(string actionName)
        {
            if (_skipRightChecking)
                return;

//            try
//            {
                if (SecurityChecker.Value == null)
                    throw new BusinessLogicCustomException(ExceptionResources.SecurityAccessDenied);

                //TODO: codereview - здесь точно должно быть SourceName?
                var entityAction = string.Format("{0}.{1}", SourceNameHelper.Instance.GetSourceName(typeof(T)), actionName);
                if (!SecurityChecker.Value.Check(entityAction))
                    throw new SecurityInsufficientPermissionsException(entityAction);
//            }
//            catch (Exception ex)
//            {
//                HandleException(ex);
//            }
        }
        #endregion
    }

    public class SecurityInsufficientPermissionsException : BusinessLogicCustomException
    {
        #region  .  Constructors  .
        public SecurityInsufficientPermissionsException() { }
        public SecurityInsufficientPermissionsException(string entityAction) : this(entityAction, null) { }
        public SecurityInsufficientPermissionsException(string entityAction, Exception innerException)
            : base(string.Format(ExceptionResources.SecurityInsufficientPermissionsToOperation, entityAction), innerException) { }
        #endregion
    }

    public interface ISecurityAccess
    {
        void SuspendRightChecking();
        void ResumeRightChecking();
    }
}
