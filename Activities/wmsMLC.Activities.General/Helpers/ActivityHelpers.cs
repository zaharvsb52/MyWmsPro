using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.General.Helpers
{
    public static class ActivityHelpers
    {
        public static void AddCacheMetadata(ICollection<RuntimeArgument> collection, NativeActivityMetadata metadata, Argument argument, string argumentname)
        {
            var result = new RuntimeArgument(argumentname, argument == null ? typeof(object) : argument.ArgumentType, argument == null ? ArgumentDirection.In : argument.Direction, true);
            metadata.Bind(argument, result);
            collection.Add(result);
        }

        public static TraceExtension GetTraceExtension(ActivityContext context)
        {
            return context.GetExtension<TraceExtension>();
        }

        public static bool UseActivityStackTrace(ActivityContext context)
        {
            var ext = GetTraceExtension(context);
            return ext != null && !ext.NotUseActivityStackTrace;
        }

        public static BpContext GetBpContext(ActivityContext context)
        {
            var dataContext = context.DataContext;
            var bpcontextproperty = GetBpContextPropertyDescriptor(dataContext);
            return bpcontextproperty != null ? (BpContext)bpcontextproperty.GetValue(dataContext) : null;
        }

        public static T GetBpContextProperty<T>(ActivityContext context, string key)
        {
            var bpcontext = GetBpContext(context);
            if (bpcontext == null)
                return default(T);
            return bpcontext.Get<T>(key);
        }

        public static IDictionary<string, object> GetBpContextProperties(ActivityContext context, IDictionary<string, object> defaultValue)
        {
            var dataContext = context.DataContext;
            var bpcontextproperty = GetBpContextPropertyDescriptor(dataContext);
            if (bpcontextproperty == null)
                return defaultValue;

            var bpContext = (BpContext) bpcontextproperty.GetValue(dataContext);
            return bpContext != null ? bpContext.Properties : null;
        }

        private static PropertyDescriptor GetBpContextPropertyDescriptor(WorkflowDataContext dataContext)
        {
            var properties = dataContext.GetProperties();
            return properties.Find(BpContext.BpContextArgumentName, true);
        }

        public static Argument CreateDefaultValue(Type type, ArgumentDirection direction)
        {
            //HACK
            switch (direction)
            {
                case ArgumentDirection.In:
                    if (type == typeof(string))
                        return new InArgument<string>();
                    if (type == typeof(int))
                        return new InArgument<int>();
                    if (type == typeof(int?))
                        return new InArgument<int?>();
                    if (type == typeof(decimal))
                        return new InArgument<decimal>();
                    if (type == typeof(decimal?))
                        return new InArgument<decimal?>();
                    if (type == typeof(double))
                        return new InArgument<double>();
                    if (type == typeof(double?))
                        return new InArgument<double?>();
                    if (type == typeof(bool))
                        return new InArgument<bool>();
                    if (type == typeof(bool?))
                        return new InArgument<bool?>();
                    if (type == typeof(DateTime))
                        return new InArgument<DateTime>();
                    if (type == typeof(DateTime?))
                        return new InArgument<DateTime?>();
                    return new InArgument<object>();

                ///////////////////////////////////
                case ArgumentDirection.InOut:
                    if (type == typeof(string))
                        return new InOutArgument<string>();
                    if (type == typeof(int))
                        return new InOutArgument<int>();
                    if (type == typeof(int?))
                        return new InOutArgument<int?>();
                    if (type == typeof(decimal))
                        return new InOutArgument<decimal>();
                    if (type == typeof(decimal?))
                        return new InOutArgument<decimal?>();
                    if (type == typeof(double))
                        return new InOutArgument<double>();
                    if (type == typeof(double?))
                        return new InOutArgument<double?>();
                    if (type == typeof(bool))
                        return new InOutArgument<bool>();
                    if (type == typeof(bool?))
                        return new InOutArgument<bool?>();
                    if (type == typeof(DateTime))
                        return new InOutArgument<DateTime>();
                    if (type == typeof(DateTime?))
                        return new InOutArgument<DateTime?>();
                    return new InOutArgument<object>();
                ///////////////////////////////////
                case ArgumentDirection.Out:
                    if (type == typeof(string))
                        return new OutArgument<string>();
                    if (type == typeof(int))
                        return new OutArgument<int>();
                    if (type == typeof(int?))
                        return new OutArgument<int?>();
                    if (type == typeof(decimal))
                        return new OutArgument<decimal>();
                    if (type == typeof(decimal?))
                        return new OutArgument<decimal?>();
                    if (type == typeof(double))
                        return new OutArgument<double>();
                    if (type == typeof(double?))
                        return new OutArgument<double?>();
                    if (type == typeof(bool))
                        return new OutArgument<bool>();
                    if (type == typeof(bool?))
                        return new OutArgument<bool?>();
                    if (type == typeof(DateTime))
                        return new OutArgument<DateTime>();
                    if (type == typeof(DateTime?))
                        return new OutArgument<DateTime?>();
                    return new OutArgument<object>();
            }
            throw new DeveloperException("Unknown argument direction {0}", direction);
        }

        public static void AcivityUseTypeValidator(NativeActivityMetadata metadata, Type type)
        {
            //if (
            //        !type.IsPrimitive &&
            //        !typeof(BusinessObject).IsAssignableFrom(type) &&
            //        type != typeof(DateTime) && type != typeof(ValueDataField)
            //    )
            //{
            //    metadata.AddValidationError(
            //        "В этом блоке нельзя использовать, выбранный Вами тип. Пожалуйста, выберите другой тип для блока.");
            //}

            if (type.IsPrimitive || type == typeof(DateTime))
            {
                metadata.AddValidationError(
                    "В этом блоке нельзя использовать, выбранный Вами тип. Пожалуйста, выберите другой тип для блока.");
            }
        }

        /// <summary>
        /// MEGA-HACK: получаем переменные из WF и выбираем значение нужной переменной.
        /// </summary>
        public static void SetWorkFlowPropertyValue(ActivityContext context, Collection<ValueDataField> fields)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (fields == null)
                throw new ArgumentNullException("fields");

            var dataContext = context.DataContext;
            var propertyDescriptorCollection = dataContext.GetProperties();

            foreach (var field in fields)
            {
                foreach (PropertyDescriptor propertyDesc in propertyDescriptorCollection)
                {
                    var key = ValueDataFieldConstants.WfPropertyName + "Value";
                    var propertyName = field.Get<object>(key) ?? ValueDataFieldConstants.GetWfVariablePropertyName(field.Value);
                    if (propertyName != null && propertyDesc.Name.Equals(propertyName))
                    {
                        field.Value = propertyDesc.GetValue(dataContext);
                        //break;
                    }
                    
                    key = ValueDataFieldConstants.WfPropertyName + "LookupFilterExt";
                    propertyName = field.Get<string>(key) ?? ValueDataFieldConstants.GetWfVariablePropertyName(field.LookupFilterExt);
                    if (propertyName != null && propertyDesc.Name.Equals(propertyName))
                    {
                        var lookupFilterExtValue = propertyDesc.GetValue(dataContext);
                        if (lookupFilterExtValue != null)
                            field.LookupFilterExt = lookupFilterExtValue.ToString();
                        //break;
                    }

                    foreach (var prop in field.Properties.Where(p => p.Value != null).ToArray())
                    {
                        propertyName = ValueDataFieldConstants.GetWfVariablePropertyName(prop.Value);
                        if (propertyName != null && propertyDesc.Name.Equals(propertyName))
                        {
                            field.Set(prop.Key, propertyDesc.GetValue(dataContext));
                        }
                    }
                }
            }
        }

        public static string ClosingWorkingDialogMessage(DataRow[] rows, string workername)
        {
            return string.Format("На {0} назначены работы. Завершить их и начать новую?{1}{2}", 
                string.IsNullOrEmpty(workername) ? "Вас" : workername, Environment.NewLine,
                    string.Join(Environment.NewLine, rows.Select(p => string.Format("'{0}' ('{1}').", p["operationname"], p["workid"]))));
        }

        public static string GetWorkflowXaml(string workflowCode)
        {
            using (var mgr = (IXamlManager<BPWorkflow>)IoC.Instance.Resolve<IBaseManager<BPWorkflow>>())
            {
                var wf = mgr.Get(workflowCode);
                if (wf == null)
                    throw new OperationException("Workflow с кодом '{0}' не существует!", workflowCode);
                var xaml = mgr.GetXaml(workflowCode);

                if (string.IsNullOrEmpty(xaml))
                    throw new DeveloperException("Получили пустой workflow.");

                return xaml;
            }
        }
    }
}