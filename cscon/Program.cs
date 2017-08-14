using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using con = System.Console;
//TODO: Add word analyzer
//TODO: Add inverse operator
namespace cscon
{
	class Program
	{
		const int maxXCount = 512;
		static List<Coset> X;
		private static Coset activeNode;

		static void Main(string[] args)
		{
			string[] Gword = { "aaa" };
			string[] Hgenerator = { };

			int[][] G = { new[] { 0, 0, 0, 0 } };
			int[][] H = { new[] { 0, 0 } };
			Coset.NumOp = 1;
			X = new List<Coset> { new Coset() };
			activeNode = X[0];

			List<int[]> tmp = new List<int[]>(G);
			tmp.AddRange(H);
			CompleteNode(tmp.ToArray());
			int pos = X.IndexOf(activeNode);
			activeNode = (pos == X.Count - 1) ? null : X[pos + 1];

			con.WriteLine("Entering main procedure");
			while (activeNode != null && X.Count < maxXCount)
			{
				CompleteNode(G);
				pos = X.IndexOf(activeNode);
				activeNode = (pos == X.Count - 1) ? null : X[pos + 1];
			}

			pauser("Enumeration complete");
			con.ForegroundColor = ConsoleColor.Green;
			for (int i = 0; i < X.Count; i++)
				con.WriteLine("X" + X[i]);
			con.ForegroundColor = ConsoleColor.White;
			con.WriteLine("Total number of cosets: " + X.Count);
			con.ReadKey();
		}

		private static void CompleteNode(int[][] Gx)
		{
			int workingIdx = activeNode.Idx;
			con.WriteLine("Analyzing X" + workingIdx);
			//TODO: implement "Lookahead" checking procedure
			for (int i = 0; i < Gx.Length; i++)
			{
				MakeDefinition(Gx[i]);
				if (activeNode.Idx == -1)
				{
					pauser("Interrupt X" + workingIdx+ " because it was removed");
					activeNode = X[0];
					return;
				}
			}
			pauser("Complete X" + workingIdx);
		}

		private static void MakeDefinition(int[] rel)
		{
			Coset prev = activeNode;
			con.WriteLine("Checking path " + printOp(rel) + " from X" + activeNode.Idx);

			for (int i = 0; i < rel.Length; i++)
			{
				int curOp = rel[i];
				//TODO: Add INVERSE operator handler
				if (prev.Relations[curOp].To == null) //no existing path
				{
					Coset newNode = new Coset();
					X.Add(newNode);
					newNode.Relations[curOp].Back = prev;
					prev.Relations[curOp].To = newNode;
					pauser("X" + prev.Idx + " with Op" + curOp + " generated new node X" + newNode.Idx);
					prev = newNode;
					continue;
				}
				// existing path
				prev = prev.Relations[curOp].To;
			}

			MergeNode(prev, activeNode);
		}

		private static void MergeNode(Coset source, Coset target)
		{
			if (source == target || source == null || target == null)
				return;
			pauser("Conflict: merging X" + source.Idx + " into X" + target.Idx);
			for (int i = 0; i < Coset.NumOp; i++)
			{
				//cut off connection and form template
				Coset sTo = source.Relations[i].To;
				Coset sBack = source.Relations[i].Back;
				Coset tTo = target.Relations[i].To;
				Coset tBack = target.Relations[i].Back;

				source.Relations[i].To = null;
				source.Relations[i].Back = null;
				if (sTo != null)
				{
					sTo.Relations[i].Back = null;
					if (tTo == null)
					{
						target.Relations[i].To = sTo;
						sTo.Relations[i].Back = target;
					}
				}
				if (sBack != null)
				{
					sBack.Relations[i].To = null;
					if (tBack == null)
					{
						target.Relations[i].Back = sBack;
						sBack.Relations[i].To = target;
					}
				}
				MergeNode(sTo, target.Relations[i].To);
				MergeNode(sBack, target.Relations[i].Back);
			}
			X.Remove(source);
			pauser("Removed X" + source.Idx);
			source.Invalidate();
		}

		class Coset
		{
			public static int NumOp;
			private static int IdCounter;
			public readonly Transform[] Relations;
			public int Idx;

			public Coset()
			{
				Idx = IdCounter++;
				Relations = new Transform[NumOp];
				for (int i = 0; i < NumOp; i++)
					Relations[i] = new Transform();
			}

			public void Invalidate()
			{
				Idx = -1;
			}

			public override string ToString()
			{
				StringBuilder str = new StringBuilder($"{Idx}: ");
				for (int i = 0; i < NumOp; i++)
					str.AppendFormat($"Op{i}({Relations[i]}) ");
				return str.ToString();
			}
		}

		struct Transform
		{
			public Coset To;
			public Coset Back;
			public override string ToString()
			{
				return $"To {To?.Idx.ToString() ?? "None"}, From {Back?.Idx.ToString() ?? "None"}";
			}
		}

		static void pauser(string info)
		{
			con.WriteLine(info);
			return;
			while (true)
			{
				var k = con.ReadKey(true);
				var c = con.ForegroundColor;
				switch (k.KeyChar)
				{
					case 'q':
						Environment.Exit(0);
						break;
					case 'x':
						con.ForegroundColor = ConsoleColor.Cyan;
						for (int i = 0; i < X.Count; i++)
						{

							con.WriteLine("X" + X[i]);
						}
						break;
					case 'a':
						con.WriteLine(activeNode.Idx);
						break;
					default:
						return;
				}
				con.ForegroundColor = c;
			}
		}

		static string printOp(int[] op)
		{
			StringBuilder st = new StringBuilder();
			for (int i = 0; i < op.Length; i++)
				st.Append(op[i]);

			return st.ToString();
		}
	}

}
