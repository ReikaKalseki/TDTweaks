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
	
		private readonly SortedDictionary<int, int> curvePoints = new SortedDictionary<int, int>();
		private readonly List<int> keys = new List<int>();
		
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
			curvePoints[at] = value;
			if (!keys.Contains(at))
				keys.Add(at);
			keys.Sort();
			return this;
		}
		
		public float getValue(int threat) { //no need to check if threat < keys[0] since keys[0] = 0
			int idx = keys.BinarySearch(threat);
			if (idx >= 0)
				return curvePoints[threat];
			int prev = ~idx-1;
			int next = prev+1;
			int y1 = curvePoints[prev];
			int y2 = curvePoints[next];
			return Mathf.Lerp(y1, y2, (threat-prev)/(float)(next-prev));
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
			curvePoints.Clear();
			keys.Clear();
			foreach (XmlNode e in from.ChildNodes) {
				if (!(e is XmlElement))
					continue;
				if (e.Name.ToLowerInvariant() == "point") {
					int threat = int.Parse(e.Attributes["threat"].Value);
					int value = int.Parse(e.Attributes["value"].Value);
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
			foreach (KeyValuePair<int, int> kvp in curvePoints) {
				createNode(to.OwnerDocument, to, kvp);
			}
		}
			
		private void createNode(XmlDocument doc, XmlElement root, KeyValuePair<int, int> kvp) {
			XmlElement node = doc.CreateElement("Point");
			node.SetAttribute("threat", kvp.Key.ToString());
			node.SetAttribute("value", kvp.Value.ToString());
			root.AppendChild(node);
		}
	}
}
