using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.UtilityClasses;
using System.Collections.Immutable;

Console.WriteLine("Creaing excel document...");

var questions = ImmutableArray.Create(

    // Likert scale ex
    new QuestionTemplate
    {
        Id = "q0",
        Question = "A Tanár érthetően magyarázza a tananyagot?",
        Category = "Osztálytermi tevékenység",
        Type = QuestionType.LikertScaleOneToFive,
        AnswerOptions = new List<string>(),
        Description = "1 = egyáltalán nem értek egyet,2 = Inkább nem értek egyet,3 = Részben egyetértek,4 = Inkább egyetértek, 5 = teljes mértékben egyetértek"
    },

    // multiple choice ex
    new QuestionTemplate
    {
        Id = "q1",
        Question = "Melyik a kedvenc tárgyad?",
        Category = "Tantárgyak",
        Type = QuestionType.MultipleChoice,
        AnswerOptions = new List<string> { "Matematika", "Fizika", "Kémia", "Biológia", "Magyar", "Román", "Informatika" },
        Description = "Válassz egy vagy több kedvenc tantárgyat"
    },

    // multiple choice other ex
    new QuestionTemplate
    {
        Id = "q2",
        Question = "Mit szerettek csinálni iskola után?",
        Category = "Iskola utáni tevékenység",
        Type = QuestionType.MultiNomialSingleChoiceOther,
        AnswerOptions = new List<string> { "Sportolni", "Aludni", "Zenéthallgatni", "Tanulni" },
        Description = "Ha van saját foglalkozásod egészítsd ki"
    },

    // single choice ex
    new QuestionTemplate
    {
        Id = "q3",
        Question = "Ebből a tantárgyból iskolán kívül:",
        Category = "Iskola utáni tevékenység",
        Type = QuestionType.MultinomialSingleChoice,
        AnswerOptions = new List<string> {
             "magánórára, egyéni felkészítőre járok",
             "csoportos felkészülésen veszek részt",
             "nem veszek részt iskolán kívüli oktatásban ebből a tantárgyból"
       },
        Description = "Válasz egy rád jellemző választ"
    },

    // openeded ex
    new QuestionTemplate
    {
        Id = "q4",
        Question = "Mit javítanál az órán?",
        Category = "Visszajelzés",
        AnswerOptions = new List<string>(),
        Type = QuestionType.OpenEnded,
        Description = "Írd le mi véleményed őszintén"
    }
);

var answers = ImmutableArray.Create(
    new QuestionAnswer { QuestionId = "q0", Answer = "1" },
    new QuestionAnswer { QuestionId = "q0", Answer = "2" },
    new QuestionAnswer { QuestionId = "q0", Answer = "3" },
    new QuestionAnswer { QuestionId = "q0", Answer = "4" },
    new QuestionAnswer { QuestionId = "q0", Answer = "5" },

    new QuestionAnswer { QuestionId = "q1", Answer = "1-7" },        
    new QuestionAnswer { QuestionId = "q1", Answer = "6" },          
    new QuestionAnswer { QuestionId = "q1", Answer = "1-2" },        
    new QuestionAnswer { QuestionId = "q1", Answer = "3" },          
    new QuestionAnswer { QuestionId = "q1", Answer = "4-5" },

    new QuestionAnswer { QuestionId = "q2", Answer = "1" },          
    new QuestionAnswer { QuestionId = "q2", Answer = "2" },         
    new QuestionAnswer { QuestionId = "q2", Answer = "3" },
    new QuestionAnswer { QuestionId = "q2", Answer = "4" }, 
    new QuestionAnswer { QuestionId = "q2", Answer = "Rajzolni" },

    new QuestionAnswer { QuestionId = "q3", Answer = "1" },
    new QuestionAnswer { QuestionId = "q3", Answer = "2" },
    new QuestionAnswer { QuestionId = "q3", Answer = "3" },
    new QuestionAnswer { QuestionId = "q3", Answer = "2" },
    new QuestionAnswer { QuestionId = "q3", Answer = "3" },


    new QuestionAnswer { QuestionId = "q4", Answer = "Több példát kellene hozni" },
    new QuestionAnswer { QuestionId = "q4", Answer = "Interaktívabb órák" },
    new QuestionAnswer { QuestionId = "q4", Answer = "Kevesebb házi feladat" }
    );
// creating metadata
var metadata = new ReportMetadata
{
    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    FileName = "test_report.xlsx",
    Author = "Explorer Consulting",
    BLOB_URI = string.Empty
};

// creating excel report document
var adminExcel = new ExcelReportDocument(metadata);


// calling compiler to create excel report
EvaluationReportCompiler.CreateRenderOfDocument(
    questions,
    adminExcel,
    answers,
    out var compiledExcel,
    out var renderTask);

// waiting for the rendering to complete
byte[] excelBytes = await renderTask;

// saving the excel file
string filePath = "test_report.xlsx";
File.WriteAllBytes(filePath, excelBytes);

Console.WriteLine($"Done! New File location: {Path.GetFullPath(filePath)}");