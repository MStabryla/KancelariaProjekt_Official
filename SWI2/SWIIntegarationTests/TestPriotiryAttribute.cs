using System;
using System.Collections.Generic;
using System.Text;

namespace XUnit.Project.Attributes
{
    public enum TestType : int
    {
        Get = 0,
        Insert = 1,
        Edit = 2,
        Remove = 3
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; private set; }
        public TestType Type { get; private set; }

        public TestPriorityAttribute(int priority, TestType type)             
        { 
            Priority = priority; 
            Type = type;
         }
    }
}