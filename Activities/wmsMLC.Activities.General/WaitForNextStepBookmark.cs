using System;
using System.Activities;

namespace wmsMLC.Activities.General
{
    /// <summary>
    /// Создание точки восстановления (закладки). Для тестирования.
    /// </summary>
    public class WaitForNextStepBookmark<T> : NativeActivity<T>
    {
        #region .  Properties  .
        public InArgument<T> MyTestObject { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        } 
        #endregion

        protected override void Execute(NativeActivityContext context)
        {
            if (MyTestObject.Get(context) == null)
                throw new Exception("MyTestObject is required");

            context.CreateBookmark("waitingForNextStepBookmark", OnReadComplete);
        }

        private void OnReadComplete(NativeActivityContext context, Bookmark bookmark, object state)
        {
            var input = (T)state;
            context.SetValue(Result, input);
        }
    }
}
