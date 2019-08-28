using System.IO;

namespace Dfc.CourseDirectory.Services.Tests.Unit.Helpers
{
    /// <summary>
    /// Streams of anonymous data in CSV format for unit testing
    /// </summary>
    public static class CsvStreams
    {
        #region Happy Files

        // Example file provided by Mark P for TDD which is expected to pass validation.
        public static Stream BasicValidFile8Rows()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Small College,,Day ,,,,");
            sw.WriteLine(",,508,2,1,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Main College,25,Day; Employer,NO,,,");
            sw.WriteLine(",,508,2,1,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Medium College,50,Day;Block;Employer,NO,,,");
            sw.WriteLine(",,508,2,1,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Small College,,Block;Employer,YES,,,");
            sw.WriteLine("225,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,EMPLOYER,,,,,YES,,");
            sw.WriteLine("164,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,EMPLOYER,,,,,NO,East Midlands;West Midlands,");
            sw.WriteLine("38,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,EMPLOYER,,,,,NO,East Midlands;West Midlands,Camden;Hackney");
            sw.Flush();

            return ms;
        }

        // File with 2 rows in it
        public static Stream Two_Rows()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        #endregion Happy Files

        #region Unhappy Files

        // File with no header row in it
        public static Stream No_Header_Row()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // File with only a header row in it
        public static Stream Only_Header_Row()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: Values for both Standard and Framework cannot be present in the same row
        public static Stream InvalidRow_StandardAndFrameworkValuesMissing()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("1,2,3,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: STANDARD_CODE must be numeric if present
        public static Stream InvalidField_STANDARD_CODE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("a,2,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: STANDARD_VERSION must be numeric if present
        public static Stream InvalidField_STANDARD_VERSION_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("1,a,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: FRAMEWORK_CODE must be numeric if present
        public static Stream InvalidField_FRAMEWORK_CODE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,a,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: FRAMEWORK_CODE must be numeric if present
        public static Stream InvalidField_FRAMEWORK_PROG_TYPE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,a,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: FRAMEWORK_PATHWAY_CODE must be numeric if present
        public static Stream InvalidField_FRAMEWORK_PATHWAY_CODE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,4,a,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: APPRENTICESHIP_INFORMATION field is required
        public static Stream InvalidField_APPRENTICESHIP_INFORMATION_Missing()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,4,5,, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: APPRENTICESHIP_INFORMATION field maximum length is 750 characters
        public static Stream InvalidField_APPRENTICESHIP_INFORMATION_751Chars()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,4,5,Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.123456789012345678901234567890123456789012345678901, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Main College,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        #endregion Unhappy Files
    }
}
