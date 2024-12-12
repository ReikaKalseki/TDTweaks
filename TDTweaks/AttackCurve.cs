using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.FortressCore;

using UnityEngine;

namespace ReikaKalseki.TDTweaks
{
	public class AttackCurve {
		
		public static readonly int MIN_THREAT = 0;
		public static readonly int MAX_THREAT = 20000;
	
		private readonly Interpolation curve = new Interpolation();
		
		public AttackCurve(int zeroValue, int maxValue) {
			addPoint(MIN_THREAT, zeroValue);
			addPoint(MAX_THREAT, maxValue);
		}
		
		public AttackCurve addPoint(int at, int value) {
			if (at < MIN_THREAT)
				throw new Exception("Threat values cannot be negative");
			else if (at > MAX_THREAT)
				throw new Exception("Threat values cannot exceeed "+MAX_THREAT);
			FUtil.log("Adding curve point "+at+", "+value);
			curve.addPoint(at, value);
			return this;
		}
		
		public float getValue(int threat) {
			return curve.getValue(threat);
		}
		
		public void load(string file) {
			XmlDocument doc = new XmlDocument();
			doc.Load(file);
			load(doc, "Points");
		}
		
		public void load(XmlDocument doc, string tag) {
			load((XmlElement)doc.GetElementsByTagName(tag)[0]);
		}
		
		public void load(XmlElement e, string tag) {
			load((XmlElement)e.GetElementsByTagName(tag)[0]);
		}
		
		public void load(XmlElement from) {
			curve.clear();
			foreach (XmlNode e in from.ChildNodes) {
				if (!(e is XmlElement))
					continue;
				if (e.Name.ToLowerInvariant() == "point") {
					int threat = (int)float.Parse(e.Attributes["threat"].Value);
					int value = (int)float.Parse(e.Attributes["value"].Value);
					addPoint(threat, value);
				}
			}
		}
		
		public void save(string file) {
			XmlDocument doc = new XmlDocument();
			save(doc, doc, "Points");
			doc.Save(file);
		}
		
		public void save(XmlDocument doc, XmlNode under, string tag) {
			XmlElement e = doc.CreateElement(tag);
			under.AppendChild(e);
			save(e);
		}
		
		public void save(XmlElement to) {
			curve.iterate(kvp => createNode(to.OwnerDocument, to, kvp));
		}
			
		private void createNode(XmlDocument doc, XmlElement root, KeyValuePair<float, float> kvp) {
			XmlElement node = doc.CreateElement("Point");
			node.SetAttribute("threat", kvp.Key.ToString("0"));
			node.SetAttribute("value", kvp.Value.ToString("0"));
			root.AppendChild(node);
		}
	}
}
