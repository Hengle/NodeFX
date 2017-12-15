import xml.etree.cElementTree as ET
import hou

node = hou.pwd().parent().type().definition()
geo = hou.pwd().geometry()
numEmitters = hou.pwd().geometry().intAttribValue("numEmitters")

root = ET.Element("root")

for i in range(0, numEmitters):
    emitterName = "emitter" + str(i)
    currentEmitter = ET.SubElement(root, emitterName)

    for attribute in geo.pointAttribs():
        key = attribute.name()
        currentAttribute = ET.SubElement(currentEmitter, "attribute")
        if attribute.dataType() == hou.attribData.Int:
            currentAttribute.set("type", "int")
            for index, attribValue in enumerate(geo.iterPoints()[i].intListAttribValue(attribute.name())):
                attributeElement = ET.SubElement(currentAttribute, "element")
                attributeElement.set("index", str(index))
                attributeElement.set("value", str(attribValue))
        if attribute.dataType() == hou.attribData.Float:
            currentAttribute.set("type", "float")
            for index, attribValue in enumerate(geo.iterPoints()[i].floatListAttribValue(attribute.name())):
                attributeElement = ET.SubElement(currentAttribute, "element")
                attributeElement.set("index", str(index))
                attributeElement.set("value", str(attribValue))
        if attribute.dataType() == hou.attribData.String:
            currentAttribute.set("type", "string")
            for index, attribValue in enumerate(geo.iterPoints()[i].stringListAttribValue(attribute.name())):
                attributeElement = ET.SubElement(currentAttribute, "element")
                attributeElement.set("index", str(index))
                attributeElement.set("value", str(attribValue))
        currentAttribute.set("name", key)

tree = ET.ElementTree(root)
tree.write("filename.xml")
