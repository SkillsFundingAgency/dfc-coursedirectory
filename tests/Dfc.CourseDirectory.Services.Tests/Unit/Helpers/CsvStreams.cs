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
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Small College,,Day ,,,,");
            sw.WriteLine(",,508,2,1,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Venue 1,25,Day; Employer,NO,,,");
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
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // File with Empty APPRENTICESHIP WEBPAGE
        public static Stream Valid_Row_Empty_APPRENTICESHIP_WEBPAGE()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text,,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // File with Empty APPRENTICESHIP URL
        public static Stream Valid_Row_Empty_CONTRACT_URL()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text,,service@college.org.uk,0121 345 6789,,CLASSROOM,Venue 1,,Day;Block,,,,");

            sw.Flush();

            return ms;
        }
        // File with Empty APPRENTICESHIP URL
        public static Stream Valid_Row_DELIVERY_METHOD_Case_Insensitive_Correct_Values()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text,,service@college.org.uk,0121 345 6789,,CLASsRooM,Venue 1,,Day;Block,No,No,,");
            sw.WriteLine("157,1,,,,some text some text some text,,service@college.org.uk,0121 345 6789,,EmPloYeR,Venue 1,,Day;Block,No,No,,");
            sw.WriteLine("157,1,,,,some text some text some text,,service@college.org.uk,0121 345 6789,,BoTh,Venue 1,,Day;Block,No,No,,");
            sw.Flush();

            return ms;
        }
        public static Stream Valid_Row_No_VENUE_Correct_Values()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text,,service@college.org.uk,0121 345 6789,,Employer,,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: STANDARD_CODE must be numeric if present
        public static Stream ValidField_STANDARD_CODES()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: STANDARD_CODE must be numeric if present
        public static Stream ValidField_FrameworkCodes_CODES()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream ValidRow_ACROSS_ENGLAND_TRUE()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Venue 1,875,Day;Block,YeS,,,");
            sw.Flush();

            return ms;
        }
        public static Stream ValidRow_ACROSS_ENGLAND_FALSE()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Venue 1,333,Day;Block,nO,,,");
            sw.Flush();

            return ms;
        }
        public static Stream ValidRow_REGION_And_SUBREGION()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Employer,Venue 1,333,Day;Block,nO,No,East Midlands;London;East of England;North East;North West;South East;South West;West Midlands;Yorkshire and The Humber,Camden");
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
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // File with only a header row in it
        public static Stream Only_Header_Row()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: Values for both Standard and Framework cannot be present in the same row
        public static Stream InvalidRow_StandardAndFrameworkValuesMissing()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("1,2,3,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: STANDARD_CODE must be numeric if present
        public static Stream InvalidField_STANDARD_CODE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("a,2,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: STANDARD_CODE must be numeric if present
        public static Stream InvalidField_STANDARD_CODE_InvalidNumber()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("666,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: STANDARD_VERSION must be numeric if present
        public static Stream InvalidField_STANDARD_VERSION_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("1,a,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: FRAMEWORK_CODE must be numeric if present
        public static Stream InvalidField_FRAMEWORK_CODE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,a,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        public static Stream InvalidField_FRAMEWORK_Values_Invalid()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,666,6,9,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: FRAMEWORK_CODE must be numeric if present
        public static Stream InvalidField_FRAMEWORK_PROG_TYPE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,a,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: FRAMEWORK_PATHWAY_CODE must be numeric if present
        public static Stream InvalidField_FRAMEWORK_PATHWAY_CODE_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,4,a,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: APPRENTICESHIP_INFORMATION field is required
        public static Stream InvalidField_APPRENTICESHIP_INFORMATION_Missing()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }

        // VALIDATION RULE: APPRENTICESHIP_INFORMATION field maximum length is 750 characters
        public static Stream InvalidField_APPRENTICESHIP_INFORMATION_751Chars()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.123456789012345678901234567890123456789012345678901, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: APPRENTICESHIP_WEBPAGE field maximum length is 255 characters
        public static Stream InvalidField_APPRENTICESHIP_WEBPAGE_256Chars()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text,Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters if yo,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: APPRENTICESHIP_WEBPAGE value must pass regex check
        public static Stream InvalidField_APPRENTICESHIP_WEBPAGE_Regex_Error_Invalid_Character()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text,www.thisisinvalid@com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: CONTACT_EMAIL must have value
        public static Stream InvalidField_CONTACT_EMAIL_Missing()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: CONTACT_EMAIL field maximum length is 255 characters
        public static Stream InvalidField_CONTACT_EMAIL_256_Chars()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters if yo,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: CONTACT_EMAIL value must pass regex check
        public static Stream InvalidField_CONTACT_EMAIL_Regex_Invalid_character()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,test@testdotcom,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_CONTACT_PHONE_Missing()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_CONTACT_PHONE_Longer_Than_30_Chars()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,111 1 1 11 1 1 1 1 1 1 111 111 11 1 1111 111 11,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_CONTACT_PHONE_NonNumeric()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,thisisnotanumber,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_CONTACT_URL_256_Chars()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters long if you don't put a space after the fullstop.Here is an example sentence that is 100 characters if yo,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();


            return ms;
        }
        public static Stream InvalidField_CONTACT_URL_Invalid_URL_Space()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,www.testemaul cd.org,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();


            return ms;
        }
        public static Stream InvalidField_CONTACT_URL_Invalid_URL_Format()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,www.te@stemaulcd.org,CLASSROOM,Venue 1,,Day;Block,,,,");
            sw.Flush();


            return ms;
        }
        public static Stream InvalidField_DELIVERY_METHOD_Missing()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_DELIVERY_METHOD_Invalid()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,invalidDeliveryMethod,Venue 1,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream Employer_DELIVERY_METHOD_For_VENUE()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,employer,Venue 1,,Day;Block,,No,,");
            sw.Flush();

            return ms;
        }
        public static Stream Invalid_Row_No_VENUE_Valid_DELIVERY_MODE()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text,,service@college.org.uk,0121 345 6789,,CLASSROOM,,,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_RADIUS_MustBeNumericIfPresent()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Venue 1,ab2,Day;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_RADIUS_NegativeNumber()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Venue 1,-999,Day;Block,No,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_RADIUS_875()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Venue 1,875,Day;Block,No,,,");
            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: STANDARD_CODE must be numeric if present
        public static Stream InvalidFile_Duplicate_STANDARD_CODES_SameDeliveryMethod_Same_Venue()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,No,,,");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Both,Venue 1,,Day;Block,No,,,");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,No,,,");


            sw.Flush();

            return ms;
        }
        // VALIDATION RULE: STANDARD_CODE must be numeric if present
        public static Stream InvalidRow_FrameworkCodes_DuplicateRows()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine(",,3,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,No,,,");
            sw.WriteLine(",,3,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Both,Venue 1,,Day;Block,No,,,");
            sw.WriteLine(",,3,4,5,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Day;Block,No,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_DELIVERY_MODE_Invalid_Option()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,Dya;Bolck,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidField_DELIVERY_MODE_Duplicate_Option()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,CLASSROOM,Venue 1,,dAy;BlOck;Block,,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidRow_ACROSS_ENGLAND_Invalid()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,BOTH,Venue 1,333,Day;Block,invalidAcross,,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidRow_NATIONAL_DELIVERY_Invalid()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Employer,Venue 1,333,Day;Block,Yes,invalidNational,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidRow_Invalid_REGION()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Employer,Venue 1,333,Day;Block,nO,No,East Mdlands;Londn;East of England;North East;North West;South East;South West;West Midlands;Yorkshire and The Humber,,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidRow_Invalid_SUBREGION()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Employer,Venue 1,333,Day;Block,nO,No,East of England;North East;North West;South East;South West;West Midlands;Yorkshire and The Humber,Camdon,");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidRow_Invalid_VENUE()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Classroom,Venue 42,333,Day;Block,nO,No,East Midlands;London;East of England;North East;North West;South East;South West;West Midlands;Yorkshire and The Humber,Camden");
            sw.Flush();

            return ms;
        }
        public static Stream InvalidRow_Multiple_VENUE()
        {
            MemoryStream ms = new MemoryStream();

            TextWriter sw = new StreamWriter(ms);
            sw.WriteLine("STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY, REGION, SUB_REGION");
            sw.WriteLine("157,1,,,,some text some text some text, http://www.bbc.com,service@college.org.uk,0121 345 6789,http://www.bbc.com/contactus,Classroom,Venue 3,333,Day;Block,nO,No,East Midlands;London;East of England;North East;North West;South East;South West;West Midlands;Yorkshire and The Humber,Camden");
            sw.Flush();

            return ms;
        }
        #endregion Unhappy Files
    }
}
