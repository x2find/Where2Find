using System;

namespace Tests
{
    public class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public DateTime? NullableDate { get; set; }
        public double Double { get; set; }
        public bool Bool { get; set; }
        public TestData Another { get; set; }
    }

    public class TestDataSub : TestData
    {
        public string SpecificToSub { get; set; }
    }
}