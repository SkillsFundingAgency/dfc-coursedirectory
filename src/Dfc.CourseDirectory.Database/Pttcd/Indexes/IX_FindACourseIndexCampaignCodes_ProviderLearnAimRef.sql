CREATE UNIQUE INDEX [IX_FindACourseIndexCampaignCodes_ProviderLearnAimRef] ON [Pttcd].[FindACourseIndexCampaignCodes] ([ProviderId], [LearnAimRef]) INCLUDE ([CampaignCodesJson])
