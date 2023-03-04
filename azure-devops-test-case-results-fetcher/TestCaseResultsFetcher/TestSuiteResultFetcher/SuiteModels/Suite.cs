
namespace TestCaseResultsFetcher.TestSuiteResultFetcher.SuiteModels
{
    internal abstract class Suite
    {
        public int Id { get; }
        public string Name { get; }

        protected Suite(int id, string name)
        {
            Id = id;
            Name = name;

        }

    }
}