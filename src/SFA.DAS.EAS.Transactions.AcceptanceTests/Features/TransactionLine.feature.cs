﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EAS.Transactions.AcceptanceTests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("TransactionLine")]
    public partial class TransactionLineFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TransactionLine.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "TransactionLine", "\tIn order to show details of my balance\r\n\tI want view transactions in and out for" +
                    " my account", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
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
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Transaction History levy declarations")]
        public virtual void TransactionHistoryLevyDeclarations()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Transaction History levy declarations", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I have an account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Paye_scheme",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction"});
            table1.AddRow(new string[] {
                        "223/ABC",
                        "1000",
                        "16-17",
                        "11",
                        "1"});
            table1.AddRow(new string[] {
                        "223/ABC",
                        "1100",
                        "16-17",
                        "12",
                        "1"});
#line 8
 testRunner.When("I have the following submissions", ((string)(null)), table1, "When ");
#line 12
 testRunner.Then("the balance should be 1210 on the screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Transaction History levy declarations with multiple schemes")]
        public virtual void TransactionHistoryLevyDeclarationsWithMultipleSchemes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Transaction History levy declarations with multiple schemes", ((string[])(null)));
#line 14
this.ScenarioSetup(scenarioInfo);
#line 15
 testRunner.Given("I have an account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Paye_scheme",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "1000",
                        "16-17",
                        "11",
                        "1"});
            table2.AddRow(new string[] {
                        "456/ABC",
                        "1000",
                        "16-17",
                        "11",
                        "1"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "1100",
                        "16-17",
                        "12",
                        "1"});
#line 16
 testRunner.When("I have the following submissions", ((string)(null)), table2, "When ");
#line 21
 testRunner.Then("the balance should be 2310 on the screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Transaction History levy declarations over Payroll_year")]
        public virtual void TransactionHistoryLevyDeclarationsOverPayroll_Year()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Transaction History levy declarations over Payroll_year", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 24
 testRunner.Given("I have an account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Paye_scheme",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction"});
            table3.AddRow(new string[] {
                        "323/ABC",
                        "1000",
                        "16-17",
                        "12",
                        "1"});
            table3.AddRow(new string[] {
                        "323/ABC",
                        "100",
                        "17-18",
                        "01",
                        "1"});
#line 25
 testRunner.When("I have the following submissions", ((string)(null)), table3, "When ");
#line 29
 testRunner.Then("the balance should be 1100 on the screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Transaction History levy declarations and Payments")]
        public virtual void TransactionHistoryLevyDeclarationsAndPayments()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Transaction History levy declarations and Payments", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 32
 testRunner.Given("I have an account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Paye_scheme",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction"});
            table4.AddRow(new string[] {
                        "423/ABC",
                        "1000",
                        "17-18",
                        "01",
                        "1"});
            table4.AddRow(new string[] {
                        "423/ABC",
                        "1100",
                        "17-18",
                        "02",
                        "1"});
#line 33
 testRunner.When("I have the following submissions", ((string)(null)), table4, "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Payment_Amount",
                        "Payment_Type"});
            table5.AddRow(new string[] {
                        "100",
                        "levy"});
            table5.AddRow(new string[] {
                        "200",
                        "cofund"});
#line 37
 testRunner.And("I have the following payments", ((string)(null)), table5, "And ");
#line 41
 testRunner.Then("the balance should be 1110 on the screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
