﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.4.0.0
//      SpecFlow Generator Version:2.4.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EmployerFinance.AcceptanceTests.Features.LateAccounts
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Account is opened on the 23 MAY 2017 and after only added valid PAYE HMRC submiss" +
        "ions no older than 12 months old")]
    public partial class AccountIsOpenedOnThe23MAY2017AndAfterOnlyAddedValidPAYEHMRCSubmissionsNoOlderThan12MonthsOldFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Scenario-01-Account-is-opened-on-the-23-MAY-2017-and-after-only-added-valid-PAYE-HMRC-submissions-no-older-than-12-months-old.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Account is opened on the 23 MAY 2017 and after only added valid PAYE HMRC submiss" +
                    "ions no older than 12 months old", null, ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Account is opened on the 23 MAY 2017 and after only added valid PAYE HMRC submiss" +
            "ions no older than 12 months old")]
        public virtual void AccountIsOpenedOnThe23MAY2017AndAfterOnlyAddedValidPAYEHMRCSubmissionsNoOlderThan12MonthsOld()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Account is opened on the 23 MAY 2017 and after only added valid PAYE HMRC submiss" +
                    "ions no older than 12 months old", null, ((string[])(null)));
#line 3
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 4
 testRunner.Given("An employer is adding a PAYE which has submissions older than the 12 month expiry" +
                    " rule limit", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Id",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate",
                        "CreatedDate"});
            table1.AddRow(new string[] {
                        "999000101",
                        "1000",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-19",
                        "2017-05-20"});
            table1.AddRow(new string[] {
                        "999000102",
                        "2000",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-19",
                        "2017-06-20"});
            table1.AddRow(new string[] {
                        "999000103",
                        "3000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-19",
                        "2017-07-20"});
            table1.AddRow(new string[] {
                        "999000104",
                        "4000",
                        "17-18",
                        "4",
                        "1",
                        "2017-08-19",
                        "2017-08-20"});
            table1.AddRow(new string[] {
                        "999000105",
                        "5000",
                        "17-18",
                        "5",
                        "1",
                        "2017-09-19",
                        "2017-09-20"});
            table1.AddRow(new string[] {
                        "999000106",
                        "6000",
                        "17-18",
                        "6",
                        "1",
                        "2017-10-19",
                        "2017-10-20"});
            table1.AddRow(new string[] {
                        "999000107",
                        "7000",
                        "17-18",
                        "7",
                        "1",
                        "2017-11-19",
                        "2017-11-20"});
            table1.AddRow(new string[] {
                        "999000108",
                        "8000",
                        "17-18",
                        "8",
                        "1",
                        "2017-12-19",
                        "2017-12-20"});
            table1.AddRow(new string[] {
                        "999000109",
                        "9000",
                        "17-18",
                        "9",
                        "1",
                        "2018-01-19",
                        "2018-01-20"});
            table1.AddRow(new string[] {
                        "999000110",
                        "10000",
                        "17-18",
                        "10",
                        "1",
                        "2018-02-19",
                        "2018-02-20"});
            table1.AddRow(new string[] {
                        "999000111",
                        "11000",
                        "17-18",
                        "11",
                        "1",
                        "2018-03-19",
                        "2018-03-20"});
            table1.AddRow(new string[] {
                        "999000112",
                        "12000",
                        "17-18",
                        "12",
                        "1",
                        "2018-04-19",
                        "2018-04-20"});
            table1.AddRow(new string[] {
                        "999000113",
                        "13000",
                        "18-19",
                        "1",
                        "1",
                        "2018-05-19",
                        "2018-05-20"});
#line 5
 testRunner.And("Hmrc return the following submissions for paye scheme", ((string)(null)), table1, "And ");
#line 20
 testRunner.When("we refresh levy data for paye scheme on the 5/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 21
 testRunner.And("all the transaction lines in this scenario have had their transaction date update" +
                    "d to their created date", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.Then("we should see a level 1 screen with a levy declared of 13200 on the 5/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 23
 testRunner.And("we should see a level 1 screen with a balance of 13200 on the 5/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion

