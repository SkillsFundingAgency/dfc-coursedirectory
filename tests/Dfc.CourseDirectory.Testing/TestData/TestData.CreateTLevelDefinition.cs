using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<Guid> CreateTLevelDefinition(Guid? tLevelDefinitionId = null, int frameworkCode = 123, int progType = 456, string name = "Test T Level")
        {
            var id = tLevelDefinitionId ?? Guid.NewGuid();

            var result = await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new CreateTLevelDefinition
            {
                TLevelDefinitionId = id,
                FrameworkCode = frameworkCode,
                ProgType = progType,
                Name = name
            }));

            return id;
        }

        public Task<IReadOnlyCollection<TLevelDefinition>> CreateInitialTLevelDefinitions() =>
            WithSqlQueryDispatcher(async dispatcher =>
            {
                var createCommands = new[]
                {
                    new CreateTLevelDefinition()
                    {
                        TLevelDefinitionId = new Guid("9555d14f-f73b-495c-91c3-4703691c6347"),
                        FrameworkCode = 36,
                        ProgType = 32,
                        Name = "T Level Digital - Digital Production, Design and Development",
                        ExemplarWhoFor = "This T Level is suitable for anyone wanting a career in software production and design.\n\nThe T Level is a 2 year programme and will include a 9 week (minimum) industry placement and a Technical Qualification.",
                        ExemplarEntryRequirements = "You will need either 5 GCSEs (at grade 4 or above), including English, maths and science, or a pass in a relevant level 2 qualification, for example a BTEC Tech Award in Digital Information Technology.\n\nIf you do not have the recommended entry qualifications, you may still be considered if you have relevant experience or show a natural ability for the subject.",
                        ExemplarWhatYoullLearn = "You will develop the skills to:\n\nAnalyse a problem, understand user needs, define requirements and set acceptance criteria\nDesign, implement and test software\nChange, maintain and support software\nWork collaboratively in a digital team\nDiscover, evaluate and apply reliable sources of knowledge\nWork within legal and regulatory frameworks when developing software",
                        ExemplarHowYoullLearn = "Your learning will combine classroom theory and practical learning and include 9 weeks (minimum) of industry placement.  The placement will provide you with a real experience of the workplace.",
                        ExemplarHowYoullBeAssessed = "You will be assessed by completing an employer set project and a project on a specialist subject. You'll have time to research and complete tasks. There'll also be 2 exams.\n\nYou'll work with your tutor to decide when you're ready to be assessed.",
                        ExemplarWhatYouCanDoNext = "You'll have the industry knowledge and experience to progress into roles like:\n\nWeb developer\nWeb designer\nIT business analyst\nApp developer\nDigital marketer\n\nor to go onto an apprenticeship or higher education."
                    },
                    new CreateTLevelDefinition()
                    {
                        TLevelDefinitionId = new Guid("c1bc1361-cda3-42a3-9120-a08cdb78dba0"),
                        FrameworkCode = 37,
                        ProgType = 31,
                        Name = "T Level Education - Education and Childcare",
                        ExemplarWhoFor = "This T Level is suitable for anyone wanting a career in early years education, childcare or assisting teaching.\n\nThe T Level is a 2 year programme and will include a 9 week (minimum) industry placement and a Technical Qualification, where you will also choose a specialist subject.",
                        ExemplarEntryRequirements = "You will need either 5 GCSEs (at grade 4 or above), including English, maths and science, or a pass in a relevant level 2 qualification.\n\nIf you do not have the recommended entry qualifications, you may still be considered if you have relevant experience or show a natural ability for the subject.",
                        ExemplarWhatYoullLearn = "You will develop an understanding of:\n\nThe education and childcare sector from ages 0 to 19\nChild development\nHow to support children and young people’s education\nSafeguarding, health and safety and wellbeing\nUnderstanding and managing behaviour\nObserving and assessing children and young people\nEquality and diversity\nSpecial educational needs and disability\nEnglish as an additional language\nWorking with parents, carers and wider families\nWorking with agencies and services that support children, families and carers\nReflective practice and other forms of professional development\n\nIn addition to the core content, you will choose one of the following as a specialist subject:\n\nEarly years education and childcare\nAssisting teaching\nSupporting and mentoring students in further and higher education (available from September 2021)",
                        ExemplarHowYoullLearn = "Your learning will combine classroom theory and practical learning and include 9 weeks (minimum) of industry placement.  The placement will provide you with real experience of the workplace.",
                        ExemplarHowYoullBeAssessed = "You will be assessed by completing employer set project and a project on your specialist subject. You'll have time to research and complete tasks. There'll also be 2 exams.\n\nYou'll work with your tutor to decide when you're ready to be assessed.",
                        ExemplarWhatYouCanDoNext = "You'll have the industry knowledge and experience to progress into roles like:\n\nNursery worker\nSpecial educational needs (SEN) teaching assistant\nTeaching assistant\nLearning mentor\n\nor to go onto an apprenticeship or higher education."
                    },
                    new CreateTLevelDefinition()
                    {
                        TLevelDefinitionId = new Guid("fc9fefe1-ee86-4df9-9d09-c275ccbf5940"),
                        FrameworkCode = 38,
                        ProgType = 31,
                        Name = "T Level Construction - Design, Surveying and Planning for Construction",
                        ExemplarWhoFor = "This T Level is suitable for anyone wanting a career in construction, specifically in surveying and design, civil engineering, building services design, or hazardous materials surveying.\n\nThe T Level is a 2 year programme and will include a 9 week (minimum) industry placement and a Technical Qualification, where you will also choose a specialist subject.",
                        ExemplarEntryRequirements = "You will need either 5 GCSEs (at grade 4 or above), including English, maths and science, or a pass in a relevant level 2 qualification.\n\nIf you do not have the recommended entry qualifications, you may still be considered if you have relevant experience or show a natural ability for the subject.",
                        ExemplarWhatYoullLearn = "You'll learn specific topics in design, surveying and planning:\n\nProject management\nBudgeting and resource allocation\nProcurement\nRisk management\n\nIn addition to the core content, you will choose one of the following as a specialist subject:\n\nSurveying and design for construction and the built environment\nCivil engineering\nBuilding services design\nHazardous materials analysis and surveying",
                        ExemplarHowYoullLearn = "Your learning will combine classroom theory and practical learning and include 9 weeks (minimum) of industry placement. The placement will provide you with a real experience of the workplace.",
                        ExemplarHowYoullBeAssessed = "You will be assessed by completing an employer set project and a project on your specialist subject. You'll have time to research and complete tasks. There'll also be 2 exams. \n\nYou'll work with your tutor to decide when you're ready to be assessed.",
                        ExemplarWhatYouCanDoNext = "You'll have the industry knowledge and experience to progress into roles like:\n\nCivil engineering technician\nTechnical surveyor\nBuilding technician\nEngineering construction technician\nArchitectural technician\n\nor go onto an apprenticeship or higher education."
                    },
                };

                foreach (var createCommand in createCommands)
                {
                    await dispatcher.ExecuteQuery(createCommand);
                }

                return await dispatcher.ExecuteQuery(new GetTLevelDefinitions());
            });
    }
}
