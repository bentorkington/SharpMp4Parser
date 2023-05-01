﻿using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace SharpMp4Parser.Muxer.Tracks.TTML
{
    public class TtmlSegmenter
    {
        public static List<Document> split(Document doc, int splitTimeInSeconds)
        {
            int splitTime = splitTimeInSeconds * 1000;
            XPathFactory xPathfactory = XPathFactory.newInstance();

            XPath xpath = xPathfactory.newXPath();
            XPathExpression xp = xpath.compile("//*[name()='p']");
            boolean thereIsMore;

            List<Document> subDocs = new List<Document>();


            do
            {
                long segmentStartTime = subDocs.Count * splitTime;
                long segmentEndTime = (subDocs.Count + 1) * splitTime;
                Document d = (Document)doc.cloneNode(true);
                NodeList timedNodes = (NodeList)xp.evaluate(d, XPathConstants.NODESET);
                thereIsMore = false;

                for (int i = 0; i < timedNodes.getLength(); i++)
                {
                    Node p = timedNodes.item(i);
                    long startTime = getStartTime(p);
                    long endTime = getEndTime(p);
                    //p.appendChild(d.createComment(toTimeExpression(startTime) + " -> " + toTimeExpression(endTime)));
                    if (startTime < segmentStartTime && endTime > segmentStartTime)
                    {
                        changeTime(p, "begin", segmentStartTime - startTime);
                        startTime = segmentStartTime;

                    }

                    if (startTime >= segmentStartTime && startTime < segmentEndTime && endTime > segmentEndTime)
                    {
                        changeTime(p, "end", segmentEndTime - endTime);
                        startTime = segmentStartTime;
                        endTime = segmentEndTime;
                    }

                    if (startTime > segmentEndTime)
                    {
                        thereIsMore = true;
                    }

                    if (!(startTime >= segmentStartTime && endTime <= segmentEndTime))
                    {
                        Node parent = p.getParentNode();
                        parent.removeChild(p);
                    }
                    else
                    {
                        changeTime(p, "begin", -segmentStartTime);
                        changeTime(p, "end", -segmentStartTime);
                    }

                }
                trimWhitespace(d);

                XPathExpression bodyXP = xpath.compile("/*[name()='tt']/*[name()='body'][1]");
                Element body = (Element)bodyXP.evaluate(d, XPathConstants.NODE);
                String beginTime = body.getAttribute("begin");
                String endTime = body.getAttribute("end");
                if (beginTime == null || "".Equals(beginTime))
                {
                    body.setAttribute("begin", toTimeExpression(segmentStartTime));
                }
                else
                {
                    changeTime(body, "begin", segmentStartTime);
                }
                if (endTime == null || "".Equals(endTime))
                {
                    body.setAttribute("end", toTimeExpression(segmentEndTime));
                }
                else
                {
                    changeTime(body, "end", segmentEndTime);
                }
                subDocs.Add(d);
            } while (thereIsMore);

            return subDocs;
        }

        public static void changeTime(Node p, string attribute, long amount)
        {
            if (p.getAttributes() != null && p.getAttributes().getNamedItem(attribute) != null)
            {
                string oldValue = p.getAttributes().getNamedItem(attribute).getNodeValue();
                long nuTime = toTime(oldValue) + amount;
                int frames = 0;
                if (oldValue.Contains("."))
                {
                    frames = -1;
                }
                else
                {
                    // todo more precision! 44 ~= 23 frames per second.
                    // that should be ok for non high framerate content
                    // actually I'd have to get the ttp:frameRateMultiplier
                    // and the ttp:frameRate attribute to calculate at which frame to show the sub
                    frames = (int)(nuTime - (nuTime / 1000) * 1000) / 44;
                }
                p.getAttributes().getNamedItem(attribute).setNodeValue(toTimeExpression(nuTime, frames));
            }
        }


        public static Document normalizeTimes(Document doc)
        {
            XPathFactory xPathfactory = XPathFactory.newInstance();

            XPath xpath = xPathfactory.newXPath();
            xpath.setNamespaceContext(TtmlHelpers.NAMESPACE_CONTEXT);
            XPathExpression xp = xpath.compile("//*[name()='p']");
            NodeList timedNodes = (NodeList)xp.evaluate(doc, XPathConstants.NODESET);
            for (int i = 0; i < timedNodes.getLength(); i++)
            {
                Node p = timedNodes.item(i);
                pushDown(p);

            }
            for (int i = 0; i < timedNodes.getLength(); i++)
            {
                Node p = timedNodes.item(i);
                removeAfterPushDown(p, "begin");
                removeAfterPushDown(p, "end");

            }
            return doc;
        }

        private static void pushDown(Node p)
        {
            long time = 0;

            Node current = p;
            while ((current = current.getParentNode()) != null)
            {
                if (current.getAttributes() != null && current.getAttributes().getNamedItem("begin") != null)
                {
                    time += toTime(current.getAttributes().getNamedItem("begin").getNodeValue());
                }
            }

            if (p.getAttributes() != null && p.getAttributes().getNamedItem("begin") != null)
            {
                p.getAttributes().getNamedItem("begin").setNodeValue(toTimeExpression(time + toTime(p.getAttributes().getNamedItem("begin").getNodeValue())));
            }
            if (p.getAttributes() != null && p.getAttributes().getNamedItem("end") != null)
            {
                p.getAttributes().getNamedItem("end").setNodeValue(toTimeExpression(time + toTime(p.getAttributes().getNamedItem("end").getNodeValue())));
            }
        }

        private static void removeAfterPushDown(Node p, string begin)
        {
            Node current = p;
            while ((current = current.getParentNode()) != null)
            {
                if (current.getAttributes() != null && current.getAttributes().getNamedItem(begin) != null)
                {
                    current.getAttributes().removeNamedItem(begin);
                }
            }
        }

        public static void trimWhitespace(Node node)
        {
            NodeList children = node.getChildNodes();
            for (int i = 0; i < children.getLength(); ++i)
            {
                Node child = children.item(i);
                if (child.getNodeType() == Node.TEXT_NODE)
                {
                    child.setTextContent(child.getTextContent().trim());
                }
                trimWhitespace(child);
            }
        }
    }
}