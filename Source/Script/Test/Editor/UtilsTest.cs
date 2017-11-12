using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEditor.VersionControl;

namespace UBlockly.Test
{
	public class UtilsTest
	{
		[Test]
		public void TestGenUid()
		{
			var uuids = new List<string>();
			for (int i = 0; i < 1000; i++)
			{
				var uuid = Utils.GenUid();
				Assert.False(uuids.Contains(uuid));
				uuids.Add(uuid);
			}
		}

		[Test]
		public void TestTokenizeInterpolation()
		{
			var tokens = Utils.TokenizeInterpolation("");
			
			Assert.IsTrue(tokens.Count == 0,"Null interpolation");

			tokens = Utils.TokenizeInterpolation("Hello");
			Assert.IsTrue(tokens.Contains("Hello"),"No interpolation");

			tokens = Utils.TokenizeInterpolation("Hello%World");
			Assert.IsTrue(tokens.Contains("Hello%World"),"Unescaped %.");

			tokens = Utils.TokenizeInterpolation("Hello%%World");
			Assert.IsTrue(tokens.Contains("Hello%World"),"Escaped %.");

			tokens = Utils.TokenizeInterpolation("Hello %1 World");
			Assert.IsTrue(string.Equals(tokens[0],"Hello ") && string.Equals(tokens[1],"1") && string.Equals(tokens[2]," World"),"Interpolation.");

			tokens = Utils.TokenizeInterpolation("%123Hello%456World%789");
			Assert.IsTrue(string.Equals(tokens[0],"123") &&
			              string.Equals(tokens[1],"Hello") &&
			              string.Equals(tokens[2],"456") && 
			              string.Equals(tokens[3],"World") &&
			              string.Equals(tokens[4],"789"),"Interpolations.");

			tokens = Utils.TokenizeInterpolation("%%%x%%0%00%01%");

			Assert.IsTrue(string.Equals(tokens[0],"%%x%0") &&
			              string.Equals(tokens[1],"0") &&
			              string.Equals(tokens[2],"1") &&
			              string.Equals(tokens[3],"%"),"Tortune interpolations.");
			
			if (!I18n.Msg.ContainsKey("STRING_REF"))
			{
				I18n.Msg.Add("STRING_REF","test string");	
			}

			tokens = Utils.TokenizeInterpolation("%{bky_string_ref}");
			Assert.IsTrue(string.Equals(tokens[0],"test string"),"String table reference,lowercase");

			tokens = Utils.TokenizeInterpolation("%{BKY_STRING_REF}");
			Assert.IsTrue(string.Equals(tokens[0],"test string"),"String table reference,uppercase");

			if (!I18n.Msg.ContainsKey("WITH_PARAM"))
			{
				I18n.Msg.Add("WITH_PARAM","before %1 after");
			}
			tokens = Utils.TokenizeInterpolation("%{bky_with_param}");
			Assert.IsTrue(string.Equals(tokens[0], "before ") &&
						  string.Equals(tokens[1],"1") &&
			              string.Equals(tokens[2]," after"),
				"String table refrence,with subreference");
			
			AddMsgToBlocklyMsg("RECURSE","before %{bky_string_ref} after");
			tokens = Utils.TokenizeInterpolation("%{bky_recurse}");
			Assert.IsTrue(string.Equals(tokens[0],"before test string after"),"String table reference,with subreference");
			
			// Error cases...
			tokens = Utils.TokenizeInterpolation("%{bky_undefined}");
			Assert.IsTrue(string.Equals(tokens[0],"%{bky_undefined}"),"Undefined string table reference");
			
			AddMsgToBlocklyMsg("1","Will not match");
			tokens = Utils.TokenizeInterpolation("before %{1} after");
			Assert.IsTrue(string.Equals("before %{1} after",tokens[0]),"Invalid initial digit in string table reference");
			
			AddMsgToBlocklyMsg("TWO WORDS","Will not match");
			tokens = Utils.TokenizeInterpolation("before %{two words} after");
			Assert.IsTrue(string.Equals(tokens[0],"before %{two words} after"),"Invalid character in string table reference: space");

			AddMsgToBlocklyMsg("TWO.WORDS","Will not match");
			tokens = Utils.TokenizeInterpolation("before %{two.words} after");
			Assert.IsTrue(string.Equals(tokens[0],"before %{two.words} after"),"Invalid character in string table reference:period");
			
			AddMsgToBlocklyMsg("AB&C","Will not match");
			tokens = Utils.TokenizeInterpolation("before %{ab&c} after");
			Assert.IsTrue(string.Equals(tokens[0],"before %{ab&c} after"),"Invalid character in string table reference: &");
			
			AddMsgToBlocklyMsg("UNCLOSED","Will not match");
			tokens = Utils.TokenizeInterpolation("before %{unclosed");
			Assert.IsTrue(string.Equals(tokens[0],"before %{unclosed"),"String table reference,with parameter");
		}


		void AddMsgToBlocklyMsg(string key, string content)
		{
			if (!I18n.Msg.ContainsKey(key))
			{
				I18n.Msg.Add(key,content);
			}
			
		}

		[Test]
		public void TestReplaceMessageReferences()
		{
			if (!I18n.Msg.ContainsKey("STRING_REF"))
			{
				I18n.Msg.Add("STRING_REF","test string");	
			}
			
			var resultString = Utils.ReplaceMessageReferences("");
			Assert.IsTrue(string.Equals("",resultString),"Empty string produces empty string");
			resultString = Utils.ReplaceMessageReferences("%{bky_string_ref}");
			Assert.IsTrue(string.Equals("test string",resultString),"Message ref dereferenced.");
			resultString = Utils.ReplaceMessageReferences("before %{bky_string_ref} after");
			Assert.IsTrue(string.Equals("before test string after",resultString),"Message ref dereferenced.");
			
			resultString = Utils.ReplaceMessageReferences("%1");
			Assert.IsTrue(string.Equals("%1",resultString),"Interpolation tokens ignored.");
			resultString = Utils.ReplaceMessageReferences("%1 %2");
			Assert.IsTrue(string.Equals("%1 %2",resultString),"Interpolation tokens ignored.");
			resultString = Utils.ReplaceMessageReferences("before %1 after");
			Assert.IsTrue(string.Equals("before %1 after",resultString),"Interpolation tokens ignored.");

			resultString = Utils.ReplaceMessageReferences("%%");
			Assert.IsTrue(string.Equals("%",resultString),"Ecaped %");
			resultString = Utils.ReplaceMessageReferences("%%{bky_string_ref}");
			Assert.IsTrue(string.Equals("%{bky_string_ref}",resultString),"Escaped %");

			resultString = Utils.ReplaceMessageReferences("%a");
			Assert.IsTrue(string.Equals("%a",resultString),"Unrecognized % escape code treated as literal");
		}

		[Test]
		public void TestCommonWordPrefix()
		{
			int len = Utils.CommonWordPrefix("one,two,three,four,five".Split(','));
			Assert.AreEqual(0, len, "No prefix");
   			
			len = Utils.CommonWordPrefix("Xone,Xtwo,Xthree,Xfour,Xfive".Split(','));
			Assert.AreEqual(0, len, "No word prefix");

			len = Utils.CommonWordPrefix("abc de,abc de,abc de,abc de".Split(','));
			Assert.AreEqual(6, len, "Full equality");
			
			len = Utils.CommonWordPrefix("abc deX,abc deY".Split(','));
			Assert.AreEqual(4, len, "One word prefix");
			
			len = Utils.CommonWordPrefix("abc de,abc deY".Split(','));
			Assert.AreEqual(4, len, "Overflow no");
			
			len = Utils.CommonWordPrefix("abc de,abc de Y".Split(','));
			Assert.AreEqual(6, len, "Overflow yes");

			len = Utils.CommonWordPrefix(new string[] {"Hello World"});
			Assert.AreEqual(11, len, "List of one");
			
			len = Utils.CommonWordPrefix(new string[] {});
			Assert.AreEqual(0, len, "Empty list");

			len = Utils.CommonWordPrefix("turn&nbsp;left,turn&nbsp;right".Split(','));
			Assert.AreEqual(0, len, "No prefix due to &amp;nbsp;");
			
			len = Utils.CommonWordPrefix("turn\u00A0left,turn\u00A0right".Split(','));
			Assert.AreEqual(0, len, "No prefix due to \\u00A0");
		}

		[Test]
		public void TestCommonWordSuffix()
		{
			int len = Utils.CommonWordSuffix("one,two,three,four,five".Split(','));
			Assert.AreEqual(0, len, "No suffix");
			
			len = Utils.CommonWordSuffix("oneX,twoX,threeX,fourX,fiveX".Split(','));
			Assert.AreEqual(0, len, "No word suffix");
			
			len = Utils.CommonWordSuffix("abc de,abc de,abc de,abc de".Split(','));
			Assert.AreEqual(6, len, "Full equality");
			
			len = Utils.CommonWordSuffix("Xabc de,Yabc de".Split(','));
			Assert.AreEqual(3, len, "One word prefix");
			
			len = Utils.CommonWordSuffix("abc de,Yabc de".Split(','));
			Assert.AreEqual(3, len, "Overflow no");
			
			len = Utils.CommonWordSuffix("abc de,Y abc de".Split(','));
			Assert.AreEqual(6, len, "Overflow yes");
			
			len = Utils.CommonWordSuffix(new string[] {"Hello World"});
			Assert.AreEqual(11, len, "List of one");
			
			len = Utils.CommonWordSuffix(new string[] {});
			Assert.AreEqual(0, len, "Empty list");
		}
	}
}
