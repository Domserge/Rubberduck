﻿using System.Linq;
using Microsoft.Vbe.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rubberduck.Inspections;
using Rubberduck.Parsing.VBA;
using Rubberduck.VBEditor.VBEInterfaces.RubberduckCodePane;
using RubberduckTests.Mocks;

namespace RubberduckTests.Inspections
{
    [TestClass]
    public class VariableIsNeverAssignedInspectionTests : VbeTestBase
    {
        [TestMethod]
        public void VariableNotAssigned_ReturnsResult()
        {
            const string inputCode =
@"Sub Foo()
    Dim var1 As String
End Sub";

            //Arrange
            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", vbext_ProjectProtection.vbext_pp_none)
                .AddComponent("Class1", vbext_ComponentType.vbext_ct_ClassModule, inputCode)
                .Build().Object;

            var codePaneFactory = new RubberduckCodePaneFactory();
            var parseResult = new RubberduckParser(codePaneFactory).Parse(project);

            var inspection = new VariableNotAssignedInspection();
            var inspectionResults = inspection.GetInspectionResults(parseResult);

            Assert.AreEqual(1, inspectionResults.Count());
        }

        [TestMethod]
        public void UnassignedVariable_ReturnsResult_MultipleVariables()
        {
            const string inputCode =
@"Sub Foo()
    Dim var1 As String
    Dim var2 As Date
End Sub";

            //Arrange
            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", vbext_ProjectProtection.vbext_pp_none)
                .AddComponent("Class1", vbext_ComponentType.vbext_ct_ClassModule, inputCode)
                .Build().Object;

            var codePaneFactory = new RubberduckCodePaneFactory();
            var parseResult = new RubberduckParser(codePaneFactory).Parse(project);

            var inspection = new VariableNotAssignedInspection();
            var inspectionResults = inspection.GetInspectionResults(parseResult);

            Assert.AreEqual(2, inspectionResults.Count());
        }

        [TestMethod]
        public void UnassignedVariable_DoesNotReturnResult()
        {
            const string inputCode =
@"Function Foo() As Boolean
    Dim var1 as String
    var1 = ""test""
End Function";

            //Arrange
            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", vbext_ProjectProtection.vbext_pp_none)
                .AddComponent("Class1", vbext_ComponentType.vbext_ct_ClassModule, inputCode)
                .Build().Object;

            var codePaneFactory = new RubberduckCodePaneFactory();
            var parseResult = new RubberduckParser(codePaneFactory).Parse(project);

            var inspection = new VariableNotAssignedInspection();
            var inspectionResults = inspection.GetInspectionResults(parseResult);

            Assert.AreEqual(0, inspectionResults.Count());
        }

        [TestMethod]
        public void UnassignedVariable_ReturnsResult_MultipleVariables_SomeAssigned()
        {
            const string inputCode =
@"Sub Foo()
    Dim var1 as Integer
    var1 = 8

    Dim var2 as String
End Sub";

            //Arrange
            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", vbext_ProjectProtection.vbext_pp_none)
                .AddComponent("Class1", vbext_ComponentType.vbext_ct_ClassModule, inputCode)
                .Build().Object;

            var codePaneFactory = new RubberduckCodePaneFactory();
            var parseResult = new RubberduckParser(codePaneFactory).Parse(project);

            var inspection = new VariableNotAssignedInspection();
            var inspectionResults = inspection.GetInspectionResults(parseResult);

            Assert.AreEqual(1, inspectionResults.Count());
        }

        [TestMethod]
        public void InspectionType()
        {
            var inspection = new VariableNotAssignedInspection();
            Assert.AreEqual(CodeInspectionType.CodeQualityIssues, inspection.InspectionType);
        }

        [TestMethod]
        public void InspectionName()
        {
            const string inspectionName = "VariableNotAssignedInspection";
            var inspection = new VariableNotAssignedInspection();

            Assert.AreEqual(inspectionName, inspection.Name);
        }
    }
}