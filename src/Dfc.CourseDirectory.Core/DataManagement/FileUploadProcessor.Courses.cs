using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task<SaveFileResult> SaveCourseFileForProvider(Guid providerId, Stream stream, UserInfo uploadedBy)
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
            return SaveFileResult.Success(new Guid(), UploadStatus.ProcessedSuccessfully);
        }

        private class CourseUploadRowValidator : AbstractValidator<CourseRow>
        { }
    }
}
