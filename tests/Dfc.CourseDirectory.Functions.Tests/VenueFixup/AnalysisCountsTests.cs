using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Functions.FixVenues;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.Functions.Tests.VenueFixup
{
    public class AnalysisCountsTests
    {
        [Fact]
        public void TestGetCountFactory()
        {
            // Arrange
            var correctionsBatch = new List<ApprenticeshipVenueCorrection>
            {
                new ApprenticeshipVenueCorrection
                {
                    ApprenticeshipLocationVenueCorrections = new List<ApprenticeshipLocationVenueCorrection>
                    {
                        new ApprenticeshipLocationVenueCorrection
                        {
                            // unfixable #1
                            CorruptionType = CorruptionType.EmptyVenueId,
                            LocationId = new Guid(),
                            VenueIdOriginal = Guid.Empty,
                            UnfixableLocationVenueReason = UnfixableLocationVenueReasons.NoMatchingVenue,
                        },
                    },
                },
                new ApprenticeshipVenueCorrection
                {
                    ApprenticeshipLocationVenueCorrections = new List<ApprenticeshipLocationVenueCorrection>
                    {
                        new ApprenticeshipLocationVenueCorrection
                        {
                            // unfixable #2
                            CorruptionType = CorruptionType.EmptyVenueId,
                            LocationId = new Guid(),
                            VenueIdOriginal = Guid.Empty,
                            UnfixableLocationVenueReason = UnfixableLocationVenueReasons.NoMatchingVenue,
                        },
                        new ApprenticeshipLocationVenueCorrection
                        {
                            // fixable #1
                            CorruptionType = CorruptionType.EmptyVenueId,
                            LocationId = new Guid(),
                            VenueIdOriginal = Guid.Empty,
                            VenueCorrection = new Venue(),
                        },
                    },
                },
                new ApprenticeshipVenueCorrection
                {
                    UnfixableVenueReason = UnfixableApprenticeshipVenueReasons.ProviderHasNoLiveVenues,
                }
            };

            // Act
            var counts = AnalysisCounts.GetCounts(correctionsBatch);

            // Assert
            using (var x = new AssertionScope())
            {
                counts.BatchSize.Should().Be(correctionsBatch.Count);
                counts.BatchSize.Should().Be(3);

                counts.CorruptLocationsAnalysed.Should().Be(
                    correctionsBatch.Sum(c => c.ApprenticeshipLocationVenueCorrections.Count));
                counts.CorruptLocationsAnalysed.Should().Be(3);

                counts.FixCounts.Should().BeEquivalentTo(new List<FixCounts>
                {
                    new FixCounts(CorruptionType.EmptyVenueId,
                        UnfixableLocationVenueReasons.NoMatchingVenue,
                        count: 2), // 2 unfixable locations in arrangement
                    new FixCounts(CorruptionType.EmptyVenueId,
                        null, // null == fixable
                        count: 1), // 1 unfixable locations arrangement
                });
            }
        }

        [Fact]
        public void TestAdd()
        {
            // Arrange
            var correctionsBatch = new List<ApprenticeshipVenueCorrection>
            {
                new ApprenticeshipVenueCorrection
                {
                    ApprenticeshipLocationVenueCorrections = new List<ApprenticeshipLocationVenueCorrection>
                    {
                        new ApprenticeshipLocationVenueCorrection
                        {
                            // unfixable #1
                            CorruptionType = CorruptionType.EmptyVenueId,
                            LocationId = new Guid(),
                            VenueIdOriginal = Guid.Empty,
                            UnfixableLocationVenueReason = UnfixableLocationVenueReasons.NoMatchingVenue,
                        },
                    },
                },
                new ApprenticeshipVenueCorrection
                {
                    ApprenticeshipLocationVenueCorrections = new List<ApprenticeshipLocationVenueCorrection>
                    {
                        new ApprenticeshipLocationVenueCorrection
                        {
                            // unfixable #2
                            CorruptionType = CorruptionType.EmptyVenueId,
                            LocationId = new Guid(),
                            VenueIdOriginal = Guid.Empty,
                            UnfixableLocationVenueReason = UnfixableLocationVenueReasons.NoMatchingVenue,
                        },
                        new ApprenticeshipLocationVenueCorrection
                        {
                            // fixable #1
                            CorruptionType = CorruptionType.EmptyVenueId,
                            LocationId = new Guid(),
                            VenueIdOriginal = Guid.Empty,
                            VenueCorrection = new Venue(),
                        },
                    },
                },
                new ApprenticeshipVenueCorrection
                {
                    UnfixableVenueReason = UnfixableApprenticeshipVenueReasons.ProviderHasNoLiveVenues,
                }
            };

            // Act
            var singleCount = AnalysisCounts.GetCounts(correctionsBatch);
            var counts = singleCount.Add(singleCount); // add to self to double all numbers (convenient quick smoke test)

            // Assert
            using (var x = new AssertionScope())
            {
                counts.BatchSize.Should().Be(correctionsBatch.Count*2);
                counts.BatchSize.Should().Be(6);

                counts.CorruptLocationsAnalysed.Should().Be(
                    correctionsBatch.Sum(c => c.ApprenticeshipLocationVenueCorrections.Count)*2);
                counts.CorruptLocationsAnalysed.Should().Be(6);

                counts.FixCounts.Should().BeEquivalentTo(new List<FixCounts>
                {
                    new FixCounts(CorruptionType.EmptyVenueId,
                        UnfixableLocationVenueReasons.NoMatchingVenue,
                        count: 4), // 2 unfixable locations in arrangement
                    new FixCounts(CorruptionType.EmptyVenueId,
                        null, // null == fixable
                        count: 2), // 1 unfixable locations arrangement
                });
            }
        }
    }
}
