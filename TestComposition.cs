using System;

// https://www.youtube.com/watch?v=nnwD5Lwwqdo
// https://scottlilly.com/c-design-patterns-composition-over-inheritance

namespace TestComposition;

static class Tester {
	public static void Go()
	{
		var msg = "TestComposition";
		Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

		var orc = new Monster(40, Monster.EAttackType.Bite | Monster.EAttackType.Spit | Monster.EAttackType.Venom);
		string canOrcBytes = orc.AttackTypes.HasFlag(Monster.EAttackType.Bite) ? "yes" : "nope";
		Console.WriteLine($"This orc has {orc.HitPoints} hit points. It has {System.Numerics.BitOperations.PopCount((uint)orc.AttackTypes)} out of {Enum.GetValues(typeof(Monster.EAttackType)).Length} possible attack types: {orc.AttackTypes}. Can it bites ? {canOrcBytes}");
	}
}

public class Monster {
	[Flags] public enum EAttackType {
		Bite = 1<<0,
		Kick = 1<<1,
		Punch = 1<<2,
		Spit = 1<<3,
		Venom = 1<<4
	}

	public EAttackType AttackTypes {get; set;}
	public uint HitPoints {get; set;}

	public Monster(uint hitPoints_a, EAttackType attackTypes_a)
	{
		HitPoints = hitPoints_a;
		AttackTypes = attackTypes_a;
	}
}