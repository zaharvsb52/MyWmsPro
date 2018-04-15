using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Processes
{
    /// <summary>
    /// Реализует механизмы запуска процессов, описанные в коде (HardCode).
    /// </summary>
    public class CodeProcessExecutorEngine : IProcessExecutorEngine
    {
        public const string ExecuteViewModelCommandName = "executeviewmodel";

        public void Run(ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
            var fullCommand = context.BpProcess.Description;
            var command = GetCommand(fullCommand, context, completedHandler);
            command.Execute();
        }

        private IBpCommand GetCommand(string fullCommand, ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
            Dictionary<string, string> commandParameters;
            var commandName = ParseCommand(fullCommand, out commandParameters);

            switch (commandName)
            {
                case ExecuteViewModelCommandName:
                    return new ExecureViewModelCommand(commandParameters, context, completedHandler);

                default:
                    throw new NotImplementedException("Unknown command " + commandName);
            }
        }

        private static string ParseCommand(string fullCommand, out Dictionary<string, string> commandParameters)
        {
            var commandParts = fullCommand.Split('?');
            if (commandParts.Length > 2)
                throw new NotSupportedException(string.Format("Unsupported command format {0}. More than one '?' char.",
                    fullCommand));

            //.Select(i => i.Split('=').ToDictionary(j => j[0], j => j[1]));
            var command = commandParts[0].ToLower().Split('=').Select(i => i).ToArray();
            if (command[0] != "command")
                throw new NotSupportedException(string.Format("Can't find 'command' in {0}.", fullCommand));

            if (string.IsNullOrEmpty(command[1]))
                throw new NotSupportedException(string.Format("Can't find command name in {0}.", fullCommand));

            commandParameters = commandParts.Length == 1
                ? new Dictionary<string, string>()
                : commandParts[1].ToLower().Split('&').Select(i => i.Split('=')).ToDictionary(k => k[0], v => v[1]);
            return command[1];
        }

        public void Run(string workflowXaml, ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
            throw new NotImplementedException();
        }
    }

    public interface IBpCommand
    {
        void Execute();
    }

    public class ExecureViewModelCommand : IBpCommand
    {
        private readonly IDictionary<string, string> _parameters;
        private readonly ExecutionContext _context;
        private readonly Action<CompleteContext> _completedHandler;
        private Type _viewModelType;

        public ExecureViewModelCommand(IDictionary<string, string> parameters, ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
            _parameters = parameters;
            _context = context;
            _completedHandler = completedHandler;

            if (!_parameters.ContainsKey("viewmodelname"))
                throw new InvalidOperationException("Не найдено обазательное свойство 'viewmodelname'.");

            var viewModelName = _parameters["viewmodelname"];
            if (string.IsNullOrEmpty(viewModelName))
                throw new InvalidOperationException("Тип ViewModel не указан.");

            _viewModelType = Type.GetType(viewModelName);
            if (_viewModelType == null)
                throw new InvalidOperationException(string.Format("Не удалось получить ViewModel по имени {0}.", viewModelName));
        }

        public void Execute()
        {
            var viewModel = IoC.Instance.Resolve(_viewModelType);
            var bpExecutor = viewModel as IBPExecutor;
            if (bpExecutor == null)
                throw new InvalidOperationException(string.Format("ViewModel, запускаемые через процессы должны реализовывать интерфейс IBPExecutor. Проверьте {0}.", viewModel.GetType()));
            bpExecutor.SetContext(_context);
            bpExecutor.Run(_completedHandler);
        }
    }

    public interface IBPExecutor
    {
        void SetContext(ExecutionContext context);
        void Run(Action<CompleteContext> completeCallback = null);
    }
}