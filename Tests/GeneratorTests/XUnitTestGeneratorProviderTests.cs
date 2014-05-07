using System.CodeDom;
using System.Linq;
using BoDi;
using Moq;
using NUnit.Framework;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser.SyntaxElements;

namespace TechTalk.SpecFlow.GeneratorTests
{
    [TestFixture]
    public class XUnitTestGeneratorProviderTests
    {
        private IObjectContainer container;
        private Mock<IUnitTestGeneratorProvider> unitTestGeneratorProviderMock;

        [SetUp]
        public void Setup()
        {
            container = GeneratorContainerBuilder.CreateContainer(new SpecFlowConfigurationHolder(), new ProjectSettings());
            container.RegisterInstanceAs<IUnitTestGeneratorProvider>(container.Resolve<XUnitTestGeneratorProvider>());
        }

        private IFeatureGenerator CreateUnitTestFeatureGenerator()
        {
            return container.Resolve<UnitTestFeatureGeneratorProvider>().CreateGenerator(new Feature());
        }

        private static Feature CreateFeature(bool featureIgnore = false, bool scenarioIgnore = false, bool scenarioOutlineIgnore = false)
        {
            var scenario1 = new Scenario("Scenario", "scenario title", "", CreateTags(scenarioIgnore), new ScenarioSteps());
            var scenario2 = new ScenarioOutline("Scenarios", "scenarios title", "", CreateTags(scenarioOutlineIgnore), new ScenarioSteps(),
                new Examples(new ExampleSet("","","",new Tags(),new GherkinTable(new GherkinTableRow(new GherkinTableCell[]{})) )));

            return new Feature("feature", "title", CreateTags(featureIgnore), "desc", null, new[] { scenario1, scenario2 }, new Comment[0]);
        }

        private static Tags CreateTags(bool ignore)
        {
            return ignore ? new Tags(new Tag("ignore")) : new Tags();
        }

        private string Name(string scenarioTitle, string attributeName, bool featureIgnore = false, bool scenarioIgnore = false, bool scenarioOutlineIgnore = false)
        {
            var generator = CreateUnitTestFeatureGenerator();
            var feature = CreateFeature(featureIgnore,scenarioIgnore,scenarioOutlineIgnore);
            var fixture = generator.GenerateUnitTestFixture(feature, "aaa", "bbb");
            var codeTypeMember = fixture.Types[0].Members.Cast<CodeTypeMember>().Single(m => m.Name == scenarioTitle);
            var attributeDeclaration =
                codeTypeMember.CustomAttributes.Cast<CodeAttributeDeclaration>().Single(a => a.Name == attributeName);
            return attributeDeclaration.Arguments[0].Name;
        }

        [Test]
        public void should_set_skip_on_fact_attribute_when_scenario_is_ignored()
        {
            var name = Name("ScenarioTitle", "Xunit.FactAttribute", scenarioIgnore: true);
            Assert.AreEqual("Skip", name);
        }

        [Test]
        public void should_set_skip_on_theory_attribute_when_scenario_outline_is_ignored()
        {
            var name = Name("ScenariosTitle", "Xunit.Extensions.TheoryAttribute", scenarioOutlineIgnore: true);
            Assert.AreEqual("Skip", name);
        }
    }
}
