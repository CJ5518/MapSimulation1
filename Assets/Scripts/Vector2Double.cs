//By Carson Rueber

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Vector2Double {
	public double x, y;
	
	//Constructor
	public Vector2Double(double x, double y) {
		this.x = x;
		this.y = y;
	}

	//Overrrides
	public override string ToString() {
		return x + ", " + y;
	}

	//Default vectors

	public static Vector2Double Zero {
		get { return new Vector2Double(0, 0); }
	}

	//Operators

	//Unary minus
	public static Vector2Double operator -(Vector2Double self) {
		return new Vector2Double(-self.x, -self.y);
	}

	//Addition
	public static Vector2Double operator +(Vector2Double first, Vector2Double second) {
		return new Vector2Double(first.x + second.x, first.y + second.y);
	}

	//Subtraction
	public static Vector2Double operator -(Vector2Double first, Vector2Double second) {
		return first + (-second);
	}

	//Multiplication
	public static Vector2Double operator *(Vector2Double self, double a) {
		return new Vector2Double(self.x * a, self.y * a);
	}
	public static Vector2Double operator *(Vector2Double self, float a) {
		return new Vector2Double(self.x * a, self.y * a);
	}

	//Division
	public static Vector2Double operator /(Vector2Double self, double a) {
		return new Vector2Double(self.x / a, self.y / a);
	}
	public static Vector2Double operator /(Vector2Double self, float a) {
		return new Vector2Double(self.x / a, self.y / a);
	}

	//Conversions

	//Vector2 and Vector2Double
	public static implicit operator Vector2Double(Vector2 vec) {
		return new Vector2Double(vec.x, vec.y);
	}
	//Made explicit because double to float is explicit
	public static explicit operator Vector2(Vector2Double vec) {
		return new Vector2((float)vec.x, (float)vec.y);
	}

	//Vector2Int and Vector2Double

	//Made explicit for the same reason as above
	public static explicit operator Vector2Int(Vector2Double vec) {
		return new Vector2Int((int)vec.x, (int)vec.y);
	}

	public static implicit operator Vector2Double(Vector2Int vec) {
		return new Vector2Double(vec.x, vec.y);
	}
}
