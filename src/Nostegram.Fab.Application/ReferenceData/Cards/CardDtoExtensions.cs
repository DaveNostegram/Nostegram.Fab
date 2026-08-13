using Nostegram.Fab.Contracts;
using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Contracts.Common;
using Nostegram.Fab.Contracts.Sets;
using Nostegram.Fab.Domain;

namespace Nostegram.Fab.Application.ReferenceData.Cards
{
    public static class CardDtoExtensions
    {
        public static CardDto ToDto(this Card card)
        {
            return new CardDto
            {
                PublicId = card.PublicId,
                Name = card.Name,
                Talents = card.Talents.Select(t => new LookupItemDto(t.PublicId, t.Name)).ToList(),
                FabClasses = card.FabClasses.Select(fc => new LookupItemDto(fc.PublicId, fc.Name)).ToList(),
                CardTypes = card.CardTypes.Select(cType => new LookupItemDto(cType.PublicId, cType.Name)).ToList(),
                CardSubTypes = card.CardSubTypes.Select(cst => new LookupItemDto(cst.PublicId, cst.Name)).ToList(),
                FlipCard = card.FlipCard != null ? new LookupItemDto(card.FlipCard.PublicId, card.FlipCard.Name) : null,
                CardVariants = card.CardVariants.Select(cv => new CardVariantDto
                {
                    PublicId = cv.PublicId,
                    CardText = cv.CardText,
                    Cost = cv.Cost,
                    Block = cv.Block,
                    Power = cv.Power,
                    Health = cv.Health,
                    Intellect = cv.Intellect,
                    Pitch = cv.Pitch.ToAPI(),
                    SetDetails = cv.SetDetails.Select(sd => new SetDetailDto
                    {
                        PublicId = sd.PublicId,
                        Rarity = sd.Rarity.ToAPI(),
                        Set = new SetDto(sd.Set.PublicId, sd.Set.Name, sd.Set.SetCode, sd.Set.ReleaseDate),
                        Artist = new LookupItemDto(sd.Artist.PublicId, sd.Artist.Name),
                        CollectorNumber = sd.CollectorNumber
                    }).ToList()
                }).ToList()
            };
        }

        public static PitchEnum ToDomain(this PitchEnumAPI value)
        {
            return value switch
            {
                PitchEnumAPI.NoPitch => PitchEnum.NoPitch,
                PitchEnumAPI.Colourless => PitchEnum.Colourless,
                PitchEnumAPI.Red => PitchEnum.Red,
                PitchEnumAPI.Yellow => PitchEnum.Yellow,
                PitchEnumAPI.Blue => PitchEnum.Blue,
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };
        }
        public static PitchEnumAPI ToAPI(this PitchEnum value)
        {
            return value switch
            {
                PitchEnum.NoPitch => PitchEnumAPI.NoPitch,
                PitchEnum.Colourless => PitchEnumAPI.Colourless,
                PitchEnum.Red => PitchEnumAPI.Red,
                PitchEnum.Yellow => PitchEnumAPI.Yellow,
                PitchEnum.Blue => PitchEnumAPI.Blue,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
        public static RarityEnum ToDomain(this RarityEnumAPI value)
        {
            return value switch
            {
                RarityEnumAPI.Common => RarityEnum.Common,
                RarityEnumAPI.Rare => RarityEnum.Rare,
                RarityEnumAPI.Majestic => RarityEnum.Majestic,
                RarityEnumAPI.Legendary => RarityEnum.Legendary,
                RarityEnumAPI.Token => RarityEnum.Token,
                RarityEnumAPI.SuperRare => RarityEnum.SuperRare,
                RarityEnumAPI.Fabled => RarityEnum.Fabled,
                RarityEnumAPI.Promo => RarityEnum.Promo,
                RarityEnumAPI.Basic => RarityEnum.Basic,
                RarityEnumAPI.Marvel => RarityEnum.Marvel,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
        public static RarityEnumAPI ToAPI(this RarityEnum value)
        {
            return value switch
            {
                RarityEnum.Common => RarityEnumAPI.Common,
                RarityEnum.Rare => RarityEnumAPI.Rare,
                RarityEnum.Majestic => RarityEnumAPI.Majestic,
                RarityEnum.Legendary => RarityEnumAPI.Legendary,
                RarityEnum.Token => RarityEnumAPI.Token,
                RarityEnum.SuperRare => RarityEnumAPI.SuperRare,
                RarityEnum.Fabled => RarityEnumAPI.Fabled,
                RarityEnum.Promo => RarityEnumAPI.Promo,
                RarityEnum.Basic => RarityEnumAPI.Basic,
                RarityEnum.Marvel => RarityEnumAPI.Marvel,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}