using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task<SaveFileResult> SaveCourseFile(Guid providerId, Stream stream, UserInfo uploadedBy)
        {
            CheckStreamIsProcessable(stream);

            if (await FileIsEmpty(stream))
            {
                return SaveFileResult.EmptyFile();
            }

            if (!await LooksLikeCsv(stream))
            {
                return SaveFileResult.InvalidFile();
            }
            //This is just a placeholder until the next ticket completes this part
            return SaveFileResult.Success(new Guid(), UploadStatus.Processed);
        }


        private class CourseUploadRowValidator : AbstractValidator<CourseRow>
        { }
    }
}
