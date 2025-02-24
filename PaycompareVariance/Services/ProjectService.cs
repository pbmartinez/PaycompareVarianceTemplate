using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PaycompareVariance.Services
{
    public class ProjectService
    {
        private readonly DTE2 _dte;
        private const string PAYCOMPARE_PROJECT = "PayCompare.Application.Engine";
        private const string PAYCOMPARE_TESTS_PROJECT = "PayCompare.UnitTests";

        public ProjectService()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = (DTE2)Package.GetGlobalService(typeof(DTE));
        }

        public string GetFilePathForValidator(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = GetPayCompareProject();

            var projectPath = Path.GetDirectoryName(project.FullName);
            var folderPath = Path.Combine(projectPath, "Services", "VarianceValidation", "Validators");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(folderPath, $"{name}.cs");
            return filePath;
        }

        public string GetFilePathForValidatorTest(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = GetProjectByName(PAYCOMPARE_TESTS_PROJECT) ?? throw new InvalidOperationException($"The project {PAYCOMPARE_TESTS_PROJECT} was not found in this solution.");

            var projectPath = Path.GetDirectoryName(project.FullName);
            var folderPath = Path.Combine(projectPath, "Application", "Engine", "Services", "VarianceValidation", "Validators");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(folderPath, $"{name}.cs");
            return filePath;
        }

        public string GetFilePathForValidatorMigration(string sequence, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = GetPayCompareProject();

            var projectPath = Path.GetDirectoryName(project.FullName);
            var folderPath = Path.Combine(projectPath, "Storage", "Migrations");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(folderPath, $"{sequence}_{name}.cs");
            return filePath;
        }

        private Project GetPayCompareProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = GetProjectByName(PAYCOMPARE_PROJECT);
            if (project == null)
            {
                throw new InvalidOperationException($"The project {PAYCOMPARE_PROJECT} was not found in this solution.");
            }
            return project;
        }

        private Project GetProjectByName(string projectName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var allProjects = GetAllProjects();
            return allProjects.FirstOrDefault(p => p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
        }

        private IEnumerable<Project> GetAllProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projects = new List<Project>();
            var solutionProjects = _dte.Solution.Projects;
            var projectEnumerator = solutionProjects.GetEnumerator();

            while (projectEnumerator.MoveNext())
            {
                var project = projectEnumerator.Current as Project;
                if (project != null)
                {
                    projects.Add(project);
                    projects.AddRange(GetSubProjects(project));
                }
            }

            return projects;
        }

        private IEnumerable<Project> GetSubProjects(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var subProjects = new List<Project>();

            if (project.ProjectItems != null)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item.SubProject != null)
                    {
                        subProjects.Add(item.SubProject);
                        subProjects.AddRange(GetSubProjects(item.SubProject));
                    }
                }
            }

            return subProjects;
        }

        public string GetContentForValidator(string name, string description)
        {
            string classContent = $@"
    using System;

    namespace PayCompare.Application.Engine.Services.VarianceValidation.Validators
    {{
        /// <summary>
        /// {description}
        /// </summary>
        public class {name}Validator : IVarianceValidator
        {{
            public Domain.Models.Enums.VarianceValidation? VarianceValidation =>
                Domain.Models.Enums.VarianceValidation.{name};

            public ValidatesField(PayCheckField field)
            {{
                throw new NotImplementedException();
            }}


            public VarianceValidationResult Validate(PayCheckVariance variance, PayrollRun payrollRun,
                (UkgProCheckData Before, UkgProCheckData AfterOriginal, UkgProCheckData After) calcData)
            {{
                var afterOriginalHelper = new UkgProCheckDataHelper(calcData.AfterOriginal);
                var afterHelper = new UkgProCheckDataHelper(calcData.After);
                
                //if conditions are met, return VarianceValidationResult.Acknowledge 
                //meaning that the variance is allowed based on the conditions expected
                //if the conditions are different from expected and  
                //this validator can't make a decision on the scenario 
                //it should return VarianceValidationResult.NoAction
                //so further validators can assess the variance and make a decision
                
                return VarianceValidationResult.NoAction;
            }}

        }}
    }}";
            return classContent;
        }

        public string GetContentForValidatorTest(string name, string description)
        {
            string classContent = $@"
    using PayCompare.Application.Engine.Services.VarianceValidation.Validators;
    using PayCompare.Domain.Models.Calculations;
    using PayCompare.Domain.Models;
    using PayCompare.Domain.Models.Enums;
    using System.Collections.Generic;
    using Xunit;

    namespace PayCompare.UnitTests.Application.Engine.Services.VarianceValidation.Validators
    {{
        /// <summary>
        /// {description}
        /// </summary>
        public class {name}ValidatorTest
        {{
            [Theory]
            //Uncomment below and complete the test
            //[InlineData(PayCheckField., true)]
            public void ShouldReturnIfValidatesField(PayCheckField field, bool expected)
            {{
                var result = new {name}Validator().ValidatesField(field);
                Assert.Equal(expected, result);
            }}

            [Theory]
            //Uncomment below and complete test cases
            //[InlineData(PayCheckField., 1, VarianceValidationResult.Acknowledge)]
            public void ShouldValidateVariance(PayCheckField field, decimal varianceAmount, VarianceValidationResult expectedResult)
            {{
                var variance = new PayCheckVariance()
                {{
                    GenNumber = ""0"",
                    PayCheckFieldId = (int)field,
                    Variance = varianceAmount
                }};
                //Complete E_Batch, D_Batch, T_Batch with the required properties as needed to test the validator
                var afterOrigCalcData = new UkgProCheckData()
                {{
                    E_Batch = new[] 
                    {{
                        new Dictionary<string, object>()
                        {{
                            {{ ""EbtGenNumber"", ""0"" }},
                        }}                    
                    }},
                    D_Batch = new[] 
                    {{ 
                        new Dictionary<string, object>()
                        {{
                            {{ ""DbtGenNumber"", ""0"" }},
                        }}
                    }},
                    T_Batch = new[] 
                    {{ 
                        new Dictionary<string, object>()
                        {{
                            {{ ""TbtGenNumber"", ""0"" }},
                        }}
                    }}
                }};

                var afterCalcData = new UkgProCheckData()
                {{
                    E_Batch = new[] 
                    {{
                        new Dictionary<string, object>()
                        {{
                            {{ ""EbtGenNumber"", ""0"" }},
                        }}                    
                    }},
                    D_Batch = new[] 
                    {{ 
                        new Dictionary<string, object>()
                        {{
                            {{ ""DbtGenNumber"", ""0"" }},
                        }}
                    }},
                    T_Batch = new[] 
                    {{ 
                        new Dictionary<string, object>()
                        {{
                            {{ ""TbtGenNumber"", ""0"" }},
                        }}
                    }}
                }};

                var result = new {name}Validator()
                    .Validate(variance, new PayrollRun(), (new UkgProCheckData(), afterOrigCalcData, afterCalcData));
                Assert.Equal(expectedResult, result);
            }}

        }}
    }}";
            return classContent;
        }

        public string GetContentForValidatorMigration(string name, string description, string migrationSequence, string migrationId, string migrationName, string migrationDesription, string migrationJira)
        {
            string classContent = $@"
    using PayCompare.Infrastructure.Storage.Migrations;
    using SimpleMigrations;

    namespace PayCompare.Application.Engine.Storage.Migrations
    {{
        /// <summary>
        /// {description}
        /// </summary>
        [Migration({migrationSequence}, ""{description}"")]
        public class {name} : PayCompareMigration
        {{
            //TODO: Remember to complete the appropriate migration id for the Variance Validation row 
            protected override void Up()
            {{
                Execute(@"" 
                    INSERT INTO VarianceValidation(Id, Name, Description, JiraId)
                    VALUES ({migrationId}, '{migrationName}', '{migrationDesription}', '{migrationJira}');
                "");
            }}

            protected override void Down()
            {{
            }}
        }}
    }}";

            return classContent;
        }

        public void WriteClassFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        public IEnumerable<string> GetFilesWithPattern(string folderPath, string pattern)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory '{folderPath}' does not exist.");
            }

            var regex = new Regex(pattern);
            var files = Directory.GetFiles(folderPath, "*.cs");
            return files.Where(file => regex.IsMatch(Path.GetFileName(file)));
        }

        public string GetSecuenceForMigration()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = GetProjectByName(PAYCOMPARE_PROJECT);
            if (project == null)
            {
                throw new InvalidOperationException("No active project found.");
            }
            var projectPath = Path.GetDirectoryName(project.FullName);
            var folderPath = Path.Combine(projectPath, "Storage", "Migrations");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var files = GetFilesWithPattern(folderPath, @"^\d{4}_");
            var lastFile = files.LastOrDefault();
            if (lastFile == null)
            {
                return "0010";
            }
            var lastFileName = Path.GetFileNameWithoutExtension(lastFile);
            var sequence = lastFileName.Split('_')[0];
            var isValidNumber = int.TryParse(sequence, out int sequenceNumber);
            return isValidNumber ? (int.Parse(sequence) + 10).ToString("D4") : string.Empty;
        }
    }
}
