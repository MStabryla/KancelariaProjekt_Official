using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;
using System.Linq;
using XUnit.Project.Attributes;

namespace SWIIntegarationTests
{
    public class TestPriority : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            
            string assemblyName = typeof(TestPriorityAttribute).AssemblyQualifiedName!;
            return testCases.OrderByDescending(testCase =>
            {
                int priority = testCase.TestMethod.Method.GetCustomAttributes(assemblyName).FirstOrDefault()?.GetNamedArgument<int>(nameof(TestPriorityAttribute.Priority)) ?? 0;
                int type = testCase.TestMethod.Method.GetCustomAttributes(assemblyName).FirstOrDefault()?.GetNamedArgument<int>(nameof(TestPriorityAttribute.Type)) ?? 0;
                return priority * 4 + type;
            });
        }
    }

    public class AlphabeticalOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
            IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
            testCases.OrderBy(testCase => testCase.TestMethod.Method.Name);
    }
}

