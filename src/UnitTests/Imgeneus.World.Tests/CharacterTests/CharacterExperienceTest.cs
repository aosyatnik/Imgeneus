using System.ComponentModel;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Monster;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterExperienceTest : BaseTest
    {
        [Fact]
        [Description("Character level should be updated with experience changes.")]
        public void LevelFromExperienceTest()
        {
            var character = CreateCharacter();

            character.TrySetMode(Mode.Ultimate);
            character.TryChangeLevel(1);
            character.TryChangeExperience(0);

            Assert.Equal((uint)0, character.Exp);

            character.TryAddExperience(201);

            Assert.Equal((uint)201, character.Exp);
            Assert.Equal(2, character.Level);
        }

        [Fact]
        [Description("Character shouldn't receive experience if already at maximum level")]
        public void ExperienceBoundaryTest()
        {
            var character = CreateCharacter();

            var maxLevel = config.Object.GetMaxLevelConfig(character.Mode).Level;

            character.TrySetMode(Mode.Ultimate);
            character.TryChangeLevel(maxLevel, true);

            Assert.Equal(maxLevel, character.Level);

            var maxLevelExp = databasePreloader.Object.Levels[(character.Mode, character.Level)].Exp;

            Assert.Equal(maxLevelExp, character.Exp);
            Assert.False(character.TryAddExperience(1));
            Assert.Equal(maxLevelExp, character.Exp);
        }

        [Fact]
        [Description("Character should receive experience by killing a mob")]
        public void ExperienceGainFromMobTest()
        {
            var map = testMap;
            var mob = new Mob(Wolf.Id, true, new MoveArea(0, 0, 0, 0, 0, 0), map, mobLoggerMock.Object, databasePreloader.Object);

            var character = CreateCharacter();

            character.TryChangeExperience(40);
            Assert.Equal((uint)40, character.Exp);

            map.LoadPlayer(character);
            map.AddMob(mob);
            mob.DecreaseHP(20000000, character);

            Assert.True(mob.IsDead);
            Assert.Equal((uint)(40 + mob.Exp), character.Exp);
        }
    }
}