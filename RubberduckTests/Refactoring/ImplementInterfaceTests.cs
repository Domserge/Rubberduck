using Moq;
using NUnit.Framework;
using Rubberduck.Interaction;
using Rubberduck.Parsing.Rewriter;
using Rubberduck.Parsing.VBA;
using Rubberduck.Refactorings;
using Rubberduck.Refactorings.ImplementInterface;
using Rubberduck.VBEditor;
using Rubberduck.VBEditor.SafeComWrappers;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;
using Rubberduck.VBEditor.Utility;
using RubberduckTests.Mocks;

namespace RubberduckTests.Refactoring
{
    [TestFixture]
    public class ImplementInterfaceTests
    {
        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_Procedure()
        {
            //Input
            const string inputCode1 =
                @"Public Sub Foo()
End Sub";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Sub Class1_Foo()
    Err.Raise 5 'TODO implement interface member
End Sub
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void DoesNotImplementInterface_SelectionNotOnImplementsStatement()
        {
            //Input
            const string inputCode1 =
                @"Public Sub Foo()
End Sub";

            const string inputCode2 =
                @"Implements Class1
   
";

            //Expectation
            const string expectedCode =
                @"Implements Class1
   
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using (state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), new Selection(2,2));

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_Procedure_ClassHasOtherProcedure()
        {
            //Input
            const string inputCode1 =
                @"Public Sub Foo()
End Sub";

            const string inputCode2 =
                @"Implements Class1

Public Sub Bar()
End Sub";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Public Sub Bar()
End Sub

Private Sub Class1_Foo()
    Err.Raise 5 'TODO implement interface member
End Sub
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PartiallyImplementedInterface()
        {
            //Input
            const string inputCode1 =
                @"Public Property Get a() As String
End Property
Public Property Let a(RHS As String)
End Property
Public Property Get b() As String
End Property
Public Property Let b(RHS As String)
End Property";

            const string inputCode2 =
                @"Implements Class1

Private Property Let Class1_b(RHS As String)
End Property";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Let Class1_b(RHS As String)
End Property

Private Property Get Class1_a() As String
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Let Class1_a(ByRef RHS As String)
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Get Class1_b() As String
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_Procedure_WithParams()
        {
            //Input
            const string inputCode1 =
                @"Public Sub Foo(ByVal a As Integer, ByRef b, c, d As Long)
End Sub";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Sub Class1_Foo(ByVal a As Integer, ByRef b As Variant, ByRef c As Variant, ByRef d As Long)
    Err.Raise 5 'TODO implement interface member
End Sub
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_Function()
        {
            //Input
            const string inputCode1 =
                @"Public Function Foo() As Integer
End Function";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Function Class1_Foo() As Integer
    Err.Raise 5 'TODO implement interface member
End Function
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_Function_WithImplicitType()
        {
            //Input
            const string inputCode1 =
                @"Public Function Foo()
End Function";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Function Class1_Foo() As Variant
    Err.Raise 5 'TODO implement interface member
End Function
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_Function_WithParam()
        {
            //Input
            const string inputCode1 =
                @"Public Function Foo(a)
End Function";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Function Class1_Foo(ByRef a As Variant) As Variant
    Err.Raise 5 'TODO implement interface member
End Function
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertyGet()
        {
            //Input
            const string inputCode1 =
                @"Public Property Get Foo() As Integer
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Get Class1_Foo() As Integer
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertyGet_WithImplicitType()
        {
            //Input
            const string inputCode1 =
                @"Public Property Get Foo()
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Get Class1_Foo() As Variant
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertyGet_WithParam()
        {
            //Input
            const string inputCode1 =
                @"Public Property Get Foo(a)
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Get Class1_Foo(ByRef a As Variant) As Variant
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertyLet()
        {
            //Input
            const string inputCode1 =
                @"Public Property Let Foo(ByRef value As Long)
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Let Class1_Foo(ByRef value As Long)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertyLet_WithParam()
        {
            //Input
            const string inputCode1 =
                @"Public Property Let Foo(a)
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Let Class1_Foo(ByRef a As Variant)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertySet()
        {
            //Input
            const string inputCode1 =
                @"Public Property Set Foo(ByRef value As Variant)
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Set Class1_Foo(ByRef value As Variant)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertySet_WithParam()
        {
            //Input
            const string inputCode1 =
                @"Public Property Set Foo(a)
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Set Class1_Foo(ByRef a As Variant)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PropertySet_AllTypes()
        {
            //Input
            const string inputCode1 =
                @"Public Sub Foo()
End Sub

Public Function Bar(ByVal a As Integer) As Boolean
End Function

Public Property Get Buz(ByVal a As Boolean) As Integer
End Property

Public Property Let Buz(ByVal a As Boolean, ByRef value As Integer)
End Property";

            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Sub Class1_Foo()
    Err.Raise 5 'TODO implement interface member
End Sub

Private Function Class1_Bar(ByVal a As Integer) As Boolean
    Err.Raise 5 'TODO implement interface member
End Function

Private Property Get Class1_Buz(ByVal a As Boolean) As Integer
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Let Class1_Buz(ByVal a As Boolean, ByRef value As Integer)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void CreatesMethodStubForAllProcedureKinds()
        {
            //Input
            const string interfaceCode =
                @"Public Sub Foo(ByVal arg1 As Integer, ByVal arg2 As String)
End Sub

Public Function Fizz(b)
End Function

Public Property Get Buzz() As Variant
End Property

Public Property Let Buzz(value)
End Property

Public Property Set Buzz(value)
End Property";

            const string inputCode =
                @"Implements IClassModule";

            //Expectation
            const string expectedCode =
                @"Implements IClassModule

Private Sub IClassModule_Foo(ByVal arg1 As Integer, ByVal arg2 As String)
    Err.Raise 5 'TODO implement interface member
End Sub

Private Function IClassModule_Fizz(ByRef b As Variant) As Variant
    Err.Raise 5 'TODO implement interface member
End Function

Private Property Get IClassModule_Buzz() As Variant
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Let IClassModule_Buzz(ByRef value As Variant)
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Set IClassModule_Buzz(ByRef value As Variant)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("IClassModule", ComponentType.ClassModule, interfaceCode)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementsInterfaceInDocumentModule()
        {
            const string interfaceCode = @"Option Explicit
Public Sub DoSomething()
End Sub
";
            const string initialCode = @"Implements IInterface";
            const string expectedCode = @"Implements IInterface

Private Sub IInterface_DoSomething()
    Err.Raise 5 'TODO implement interface member
End Sub
";

            var vbe = new MockVbeBuilder()
                .ProjectBuilder("TestProject", ProjectProtection.Unprotected)
                .AddComponent("IInterface", ComponentType.ClassModule, interfaceCode)
                .AddComponent("Sheet1", ComponentType.Document, initialCode, Selection.Home)
                .AddProjectToVbeBuilder()
                .Build();

            var project = vbe.Object.VBProjects[0];
            var component = project.VBComponents["Sheet1"];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementsInterfaceInUserFormModule()
        {
            const string interfaceCode = @"Option Explicit
Public Sub DoSomething()
End Sub
";
            const string initialCode = @"Implements IInterface";
            const string expectedCode = @"Implements IInterface

Private Sub IInterface_DoSomething()
    Err.Raise 5 'TODO implement interface member
End Sub
";

            var vbe = new MockVbeBuilder()
                .ProjectBuilder("TestProject", ProjectProtection.Unprotected)
                .AddComponent("IInterface", ComponentType.ClassModule, interfaceCode)
                .AddComponent("Form1", ComponentType.UserForm, initialCode, Selection.Home)
                .AddProjectToVbeBuilder()
                .Build();

            var project = vbe.Object.VBProjects[0];
            var component = project.VBComponents["Form1"];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [TestCase(@"Public Foo As Long")]
        [TestCase(@"Dim Foo As Long")]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PublicIntrinsic(string inputCode1)
        {
            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Get Class1_Foo() As Long
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Let Class1_Foo(ByVal rhs As Long)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [TestCase(@"Public Foo As Object")]
        [TestCase(@"Dim Foo As Object")]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PublicObject(string inputCode1)
        {
            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Get Class1_Foo() As Object
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Set Class1_Foo(ByVal rhs As Object)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [TestCase(@"Public Foo As Variant")]
        [TestCase(@"Public Foo")]
        [TestCase(@"Dim Foo As Variant")]
        [TestCase(@"Dim Foo")]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementInterface_PublicVariant(string inputCode1)
        {
            const string inputCode2 =
                @"Implements Class1";

            //Expectation
            const string expectedCode =
                @"Implements Class1

Private Property Get Class1_Foo() As Variant
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Let Class1_Foo(ByVal rhs As Variant)
    Err.Raise 5 'TODO implement interface member
End Property

Private Property Set Class1_Foo(ByVal rhs As Variant)
    Err.Raise 5 'TODO implement interface member
End Property
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using(state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), Selection.Home);

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        [Test]
        [Category("Refactorings")]
        [Category("Implement Interface")]
        public void ImplementsCorrectInterface_MultipleInterfaces()
        {
            //Input
            const string inputCode1 =
                @"Public Sub Foo()
End Sub";

            const string inputCode2 =
                @"Implements Class1
Implements Class3";

            const string inputCode3 =
                @"Public Sub Foo()
End Sub";

            //Expectation
            const string expectedCode =
                @"Implements Class1
Implements Class3

Private Sub Class1_Foo()
    Err.Raise 5 'TODO implement interface member
End Sub
";

            var builder = new MockVbeBuilder();
            var project = builder.ProjectBuilder("TestProject1", ProjectProtection.Unprotected)
                .AddComponent("Class1", ComponentType.ClassModule, inputCode1)
                .AddComponent("Class2", ComponentType.ClassModule, inputCode2)
                .AddComponent("Class3", ComponentType.ClassModule, inputCode3)
                .Build();
            var vbe = builder.AddProject(project).Build();
            var component = project.Object.VBComponents[1];

            var (state, rewritingManager) = MockParser.CreateAndParseWithRewritingManager(vbe.Object);
            using (state)
            {

                var qualifiedSelection = new QualifiedSelection(new QualifiedModuleName(component), new Selection(1,1));

                var refactoring = TestRefactoring(vbe.Object, rewritingManager, state);
                refactoring.Refactor(qualifiedSelection);

                var actualCode = component.CodeModule.Content();
                Assert.AreEqual(expectedCode, actualCode);
            }
        }

        private static IRefactoring TestRefactoring(IVBE vbe, IRewritingManager rewritingManager, RubberduckParserState state, IMessageBox msgBox = null)
        {
            var selectionService = MockedSelectionService(vbe.GetActiveSelection());
            if (msgBox == null)
            {
                msgBox = new Mock<IMessageBox>().Object;
            }
            return new ImplementInterfaceRefactoring(state, msgBox, rewritingManager, selectionService);
        }

        private static ISelectionService MockedSelectionService(QualifiedSelection? initialSelection)
        {
            QualifiedSelection? activeSelection = initialSelection;
            var selectionServiceMock = new Mock<ISelectionService>();
            selectionServiceMock.Setup(m => m.ActiveSelection()).Returns(() => activeSelection);
            selectionServiceMock.Setup(m => m.TrySetActiveSelection(It.IsAny<QualifiedSelection>()))
                .Returns(() => true).Callback((QualifiedSelection selection) => activeSelection = selection);
            return selectionServiceMock.Object;
        }
    }
}

