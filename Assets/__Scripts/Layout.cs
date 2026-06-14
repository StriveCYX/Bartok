using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;

//// SlotDef类并非MonoBehaviour的子类，因此不需要单独创建一个C#文件
//[System.Serializable] // This makes SlotDefs visible in the Unity Inspector pane
//public class SlotDef
//{
//    public float x;
//    public float y;
//    public bool faceUp = false;
//    public string layerName = "Default";
//    public int layerID = 0;
//    public int id;
//    public List<int> hiddenBy = new List<int>();
//    public string type = "slot";
//    public Vector2 stagger;
//}
public class Layout : MonoBehaviour
{
    public XDocument xmlr; // Just like Deck, this has a PT_XMLReader 
    //public PT_XMLHashtable xml; // This variable is for faster xml access
    public XElement root;
    public Vector2 multiplier; // The offset of the tableau's center 设置牌面中心距离
    // SlotDef references 
    public List<SlotDef> slotDefs; // All the SlotDefs for Row0-Row3 
    public SlotDef drawPile;
    public SlotDef discardPile;
    // This holds all of the possible names for the layers set by layerID 
    public string[] sortingLayerNames = new string[] { "Row0", "Row1",
"Row2", "Row3", "Discard", "Draw" };

    // This function is called to read in the LayoutXML.xml file 
    public void ReadLayout(string xmlText)
    {
        //xmlr.Parse(xmlText); // The XML is parsed 
        //xml = xmlr.xml["xml"][0]; // And xml is set as a shortcut to the XML 
        //                          // Read in the multiplier, which sets card spacing 
        //multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        //multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        xmlr = XDocument.Parse(xmlText);
        root = xmlr.Element("xml");
        multiplier.x = float.Parse(root.Element("multiplier").Attribute("x").Value);
        multiplier.y = float.Parse(root.Element("multiplier").Attribute("y").Value);

        // Read in the slots 
        SlotDef tSD;
        // slotsX is used as a shortcut to all the <slot>s 
        IEnumerable<XElement> slotsX = root.Elements("slot");
        foreach (XElement slot in slotsX)
        {
            tSD = new SlotDef(); // Create a new SlotDef instance 
            if(slot.Attribute("type")!=null)
            {
                // If this <slot> has a type attribute parse it 
                tSD.type = slot.Attribute("type").Value;
            }
            else
            {
                // If not, set its type to "slot"; it's a card in the rows
                // 如果没有type属性，则将type设置为“slot”，表示场景中的纸牌
                tSD.type = "slot";
            }
            // Various attributes are parsed into numerical values 
            tSD.x = float.Parse(slot.Attribute("x").Value);
            tSD.y = float.Parse(slot.Attribute("y").Value);
            tSD.layerID = int.Parse(slot.Attribute("layer").Value);
            // This converts the number of the layerID into a text layerName 
            tSD.layerName = sortingLayerNames[tSD.layerID]; // a 
            switch (tSD.type)
            {
                // pull additional attributes based on the type of this <slot> 
                case "slot":
                    tSD.faceUp = (slot.Attribute("faceup").Value == "1");
                    tSD.id = int.Parse(slot.Attribute("id").Value);
                    if (slot.Attribute("hiddenby")!=null)
                    {
                        string[] hiding = slot.Attribute("hiddenby").Value.Split(',')
                                .Select(s => s.Trim())  // 对每个字符串调用 Trim 方法
                                .ToArray();            // 转换为数组

                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slot.Attribute("xstagger").Value);
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }
}