﻿enum CNodeType {
    Empty,
    Element,
    Comment,
    Content,
    Null
}

class CNode {
    TagName: String;
    Text: String
    NodeType: CNodeType;
    Parent: CNode;
    Nodes: Array<CNode>;
    CloseNode: CNode;
    IsClosingTag: boolean;
    constructor(i: String, j: CNodeType, p: CNode) {
        this.CloseNode = null;
        this.TagName = "";
        this.IsClosingTag = false;
        this.Text = i;
        this.NodeType = j;
        this.Parent = p;
        this.Nodes = [];
    }
}

class HTMLFormatter {
    input: string;
    output: string;
    htmltemp: string;
    nodelist: Array<CNode>;
    nodeTree: CNode;

    constructor(rawhtml: string) {
        this.input = rawhtml;
        this.htmltemp = "";
        this.nodelist = [];
        this.nodeTree = new CNode("", CNodeType.Element, null);
    }

    process(): void {
        this.buildlist(this.input);

        this.generateHTML(this.nodeTree, "");
        this.output = this.htmltemp;
        console.log(this.nodeTree);

    }

    generateHTML(n: CNode, indent: String): void {
        if (n.Nodes.length > 0) {
            for (var i = 0; i < n.Nodes.length; i++) {
                this.htmltemp += indent + "" + n.Nodes[i].Text + "\n";
                this.generateHTML(n.Nodes[i], indent + "\t");
                if (n.Nodes[i].CloseNode != null) {
                    this.htmltemp += indent + "" + n.Nodes[i].CloseNode.Text + "\n";
                }
            }

        }
    }

    buildlist(rawinput: String): void {
        var temp: String;
        for (var i = 0; i < rawinput.length;) {
            var n: CNode = this.getLastOpenNode(CNodeType.Element, this.nodeTree);

            //check if it is tag opening
            if (rawinput[i] == "<") {
                //get index of tag closing bracket
                var j = rawinput.indexOf(">");
                //read tag 
                temp = rawinput.substring(i, j + 1);

                //alternatively see if the tag open leads to comment tag
                if (rawinput.substr(i, rawinput.length).length >= 4 && rawinput.substr(i, 4) == "<!--") {
                    j = rawinput.indexOf("<!--") + 4;
                    //read comment opening
                    temp = rawinput.substring(i, j);
                    //reduce raw string, trim the read part from it
                    rawinput = rawinput.substring(j, rawinput.length);
                }
                else {
                    rawinput = rawinput.substring(j + 1, rawinput.length);
                }

                var c: CNode = new CNode(temp.trim(), CNodeType.Element, n);
                c.TagName = this.extractTagName(c.Text);
                c.IsClosingTag = this.isClosingTag(temp.trim());
                console.log(c.TagName);
                this.nodelist.push(c);

                if (this.isSelfClosingTag(c.Text)) {
                    c.CloseNode = new CNode("", CNodeType.Null, n);
                    n.Nodes.push(c);
                }
                else if (c.IsClosingTag) {
                    var ntemp = n;
                    while (ntemp.Parent != null) {
                        if (ntemp.TagName == c.TagName && ntemp.CloseNode == null) {
                            ntemp.CloseNode = c;
                            break;
                        } else {
                            ntemp = ntemp.Parent;
                        }
                    }
                }
                else {
                    n.Nodes.push(c);
                }
            }
            //else part reads content be it script, html comment or text
            else {
                // if last opened tag was script tag
                if (n.NodeType == CNodeType.Element && n.TagName == "script") {
                    rawinput = this.extractScriptTagContent(n, rawinput);
                }
                else {
                    //see when the next tag begins
                    var j = rawinput.indexOf("<");
                    //if there is no further tags opening, use full remaining length
                    if (j == -1) {
                        j = rawinput.length;
                    }

                    //see if there is a comment closing tag
                    if (rawinput.substring(0, j).indexOf("-->") > -1) {
                        //read till the comment closing
                        temp = rawinput.substring(i, rawinput.indexOf("-->"));
                        if (temp.trim() != "") {
                            var c: CNode = new CNode(temp.trim(), CNodeType.Content, n);
                            c.IsClosingTag = false;
                            this.nodelist.push(c);
                            if (c.IsClosingTag) {
                                c.Parent = n.Parent;
                                n.Parent.Nodes.push(c);
                            } else {
                                n.Nodes.push(c);
                            }
                        }
                        //trim the string
                        rawinput = rawinput.substring(rawinput.indexOf("-->") + 3, rawinput.length);

                        //add comment closing tag to 
                        var c2: CNode = new CNode("-->", CNodeType.Comment, n);
                        c2.TagName = "--";
                        c2.IsClosingTag = true;
                        var ntemp = this.getOpenParentTagByName(n, "!--");
                        ntemp.CloseNode = c2;
                        c2.Parent = ntemp.Parent
                    } else {
                        //read content till a tag open of string ends
                        temp = rawinput.substring(i, j);
                        if (temp.trim() != "") {
                            var c: CNode = new CNode(temp.trim(), CNodeType.Content, n);
                            c.IsClosingTag = false;
                            this.nodelist.push(c);

                            if (c.IsClosingTag) {
                                c.Parent = n.Parent;
                                n.Parent.Nodes.push(c);
                            } else {
                                n.Nodes.push(c);
                            }

                        }
                        rawinput = rawinput.substring(j, rawinput.length);
                    }
                }

            }
        }
    }

    extractScriptTagContent(n:CNode, rawinput:String): String {
        //read script tag content
        var scrcontent: String = "";
        //store string beginger identifeir like ' or "
        var stringbegin: String = "";
        //reading raw input
        while (rawinput.length > 0) {
            //if its start of script single line comment then read line till new line
            if (this.endsWith(scrcontent, "//")) {
                var nlindex = rawinput.indexOf("\n");
                if (nlindex > -1) {
                    scrcontent += rawinput.substr(0, nlindex);
                    //trim raw input
                    rawinput = rawinput.substr(nlindex, rawinput.length);
                }
            }
            //if its start of script mutli line comment then read line till comment end tag */
            else if (this.endsWith(scrcontent, "/*")) {
                    var nlindex = rawinput.indexOf("*/");
                    if (nlindex > -1) {
                        scrcontent += rawinput.substr(0, nlindex + 2);
                        //trim raw input
                        rawinput = rawinput.substr(nlindex + 2, rawinput.length);
                    }
                }
            //if it is starting of tag and stringbegin is false
            else if (rawinput[0] === "<" && stringbegin === "") {
                break;
            }
            //if raw string start with ' it might be beging or end of string in script
            else if (rawinput[0] == "'") {
                //see if string begin is false, then set it to true with ' identifier
                if (stringbegin === "") {
                    stringbegin = "'";
                }
                //see if a string already being read
                else if (stringbegin === "'") {
                    //see if last 2 chars read were not escape sequnce, then set string begin to false
                    if (scrcontent.length >= 2 && !this.endsWith(scrcontent, "\\\\")) {
                        stringbegin = "";
                    }
                }
                //I dont know if this should be used.
                else if (stringbegin === '"') {
                }
                // read and store first character
                scrcontent += rawinput[0];
                //trim raw input
                rawinput = rawinput.substr(1, rawinput.length);
            }
            //if raw string start with " it might be beging or end of string in script
            //see above comment to understand the logic here
            else if (rawinput[0] == '"') {
                if (stringbegin === "") {
                    stringbegin = '"';
                } else if (stringbegin === '"') {
                    if (scrcontent.length >= 2 && !this.endsWith(scrcontent, "\\\\")) {
                        stringbegin = "";
                    }
                } else if (stringbegin === "'") {
                }
                scrcontent += rawinput[0];
                rawinput = rawinput.substr(1, rawinput.length);
            } else {
                scrcontent += rawinput[0];
                rawinput = rawinput.substr(1, rawinput.length);
            }
        }
        //time to store script read in the node tree
        if (scrcontent.trim() != "") {
            var c: CNode = new CNode(scrcontent.trim(), CNodeType.Content, n);
            c.IsClosingTag = false;
            this.nodelist.push(c);

            if (c.IsClosingTag) {
                c.Parent = n.Parent;
                n.Parent.Nodes.push(c);
            } else {
                n.Nodes.push(c);
            }

        }

        return rawinput;
    }

    getLastNode(t: CNodeType, start: CNode): CNode {
        var result: CNode;
        if (start.Nodes.length > 0) {
            for (var i = start.Nodes.length - 1; i >= 0; i--) {
                if (start.Nodes[i].NodeType == t) {
                    result = this.getLastNode(t, start.Nodes[i]);
                    break;
                }
            }

        }

        if (result == undefined) {
            if (start.NodeType == t || start.NodeType == CNodeType.Empty) {
                result = start;
            }
        }
        return result;
    }

    getLastOpenNode(t: CNodeType, start: CNode): CNode {
        var result: CNode;
        if (start.Nodes.length > 0 && start.CloseNode == null) {
            for (var i = start.Nodes.length - 1; i >= 0; i--) {
                if (start.Nodes[i].NodeType == t && start.Nodes[i].CloseNode == null) {
                    result = this.getLastOpenNode(t, start.Nodes[i]);
                    break;
                }
            }

        }

        if (result == undefined) {
            if (start.NodeType == t || start.NodeType == CNodeType.Empty) {
                result = start;
            }
        }
        return result;
    }

    getOpenParentTagByName(start: CNode, n: String): CNode {
        var result: CNode = null;
        while (start.Parent != null) {
            if (start.TagName.toLowerCase() == n.toLowerCase() && start.CloseNode == null) {
                //ntemp.CloseNode = c;
                //c.Parent = ntemp.Parent
                result = start;
                break;
            } else {
                start = start.Parent;
            }
        }
        return result;
    }

    extractTagName(tag: String): String {
        var result: String = "";
        if (tag.length >= 4 && tag[1] == "!" && tag[2] == "-" && tag[3] == "-") {
            result = "!--"
        }
        else if (tag.length >= 4 && tag[1] == "/" && tag.indexOf(" ") == -1 && tag[tag.length - 1] == ">") {
            result = tag.substring(2, tag.indexOf(">"));
        }
        else if (tag.length > 2 && tag.indexOf(" ") > -1) {
            result = tag.substring(1, tag.indexOf(" "));
        }
        else if (tag.length > 2 && tag.indexOf(">") > -1) {
            result = tag.substring(1, tag.indexOf(">"));
        }

        return result;
    }

    startsWith(inp: String, com: String): boolean {
        if (inp.length < com.length) {
            return false;
        } else if (inp.substring(0, com.length) == com) {
            return true;
        } else {
            return false;
        }
    }

    endsWith(inp: String, com: String): boolean {
        if (inp.length >= com.length) {
            if (inp.substr(inp.length - com.length, com.length) == com) {
                return true;
            }
            else {
                return false;
            }
        } else {
            return false;
        }
    }

    isClosingTag(s: String): boolean {
        s = s.trim();
        if (s.length > 3) {
            if (s[0] == "<" && s[1] == "/" && s[s.length - 1] == ">") {
                return true;
            } else if (s[0] == "<" && s[s.length - 1] == ">" && s[s.length - 2] == "/") {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    isSelfClosingTag(s: String): boolean {
        var tn = this.extractTagName(s).toLowerCase();
        if (tn == "br" || tn == "meta" || tn == "hr" || tn == "input" || tn == "link" || tn == "!doctype" || tn == "base") {
            return true;
        } else {
            return false;
        }
    }
}

function formatHTML() {
    var i: HTMLInputElement = <HTMLInputElement>document.getElementById("input");
    var objformat: HTMLFormatter = new HTMLFormatter(i.value);

    objformat.process();
    var i2: HTMLInputElement = <HTMLInputElement>document.getElementById("output");
    i2.value = objformat.output;
}