//-----------------------------------------------------------------------
//-----------------------------------------------------------------------


using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;

namespace NUnit.Framework
{
    /// <summary>
    /// A simple ExpectedExceptionAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExpectedExceptionAttribute : NUnitAttribute, IWrapTestMethod
    {
        private readonly Type _expectedExceptionType;

        public string ExpectedMessage { get; set; }

        public ExpectedExceptionAttribute(Type type)
        {
            _expectedExceptionType = type;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ExpectedExceptionCommand(command, _expectedExceptionType, ExpectedMessage);
        }

        private class ExpectedExceptionCommand : DelegatingTestCommand
        {
            private readonly Type _expectedType;
            private readonly string _expectedMessage;

            public ExpectedExceptionCommand(TestCommand innerCommand, Type expectedType, string expectedMessage)
                : base(innerCommand)
            {
                _expectedType = expectedType;
                _expectedMessage = expectedMessage;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                Type caughtType = null;
                string caughtMessage = null;

                try
                {
                    innerCommand.Execute(context);
                }
                catch (Exception ex)
                {
                    if (ex is NUnitException)
                        ex = ex.InnerException;
                    caughtType = ex.GetType();
                    caughtMessage = ex.Message;
                }

                if (caughtType == _expectedType)
                {
                    if (!string.IsNullOrEmpty(_expectedMessage))
                    {
                        if (caughtMessage == _expectedMessage)
                            context.CurrentResult.SetResult(ResultState.Success);
                        else
                            context.CurrentResult.SetResult(ResultState.Failure,
                                string.Format("Expected type matched. Expected message '{0}' but got '{1}'", _expectedMessage, caughtMessage));
                    }
                }
                else if (caughtType != null)
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected {0} but got {1}", _expectedType.Name, caughtType.Name));
                else
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected {0} but no exception was thrown", _expectedType.Name));

                return context.CurrentResult;
            }
        }
    }
}