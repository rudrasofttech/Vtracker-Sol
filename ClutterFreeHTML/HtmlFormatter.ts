enum CNodeType {
    Empty,
    Element,
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

    buildlist(rawinput: string): void {
        var temp: String;
        for (var i = 0; i < rawinput.length;) {
            var n: CNode = this.getLastOpenNode(CNodeType.Element, this.nodeTree);

            //check if it is tag opening
            if (rawinput[i] == "<") {
                var j = rawinput.indexOf(">");
                temp = rawinput.substring(i, j + 1);
                if (this.startsWith(temp, "<script") && temp.indexOf("src") == -1) {
                    //read full script and store it
                    j = rawinput.indexOf("</script");
                    temp = rawinput.substring(i, j + 9);
                    rawinput = rawinput.substring(j + 9, rawinput.length);
                } else {
                    rawinput = rawinput.substring(j + 1, rawinput.length);
                }
                var c: CNode = new CNode(temp.trim(), CNodeType.Element, n);
                c.IsClosingTag = this.isClosingTag(temp.trim());
                this.nodelist.push(c);

                if (this.isSelfClosingTag(c.Text)) {
                    c.TagName = this.extractTagName(c.Text);
                    c.CloseNode = new CNode("", CNodeType.Null, n);
                    n.Nodes.push(c);
                }
                else if (c.IsClosingTag) {
                    c.TagName = this.extractTagName(c.Text);
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
                    c.TagName = this.extractTagName(c.Text);
                    n.Nodes.push(c);
                }
            }

            else {
                var j = rawinput.indexOf("<");
                if (j == -1) {
                    j = rawinput.length;
                }
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

    extractTagName(tag: String): String {
        var result: String = "";
        if (tag.length > 4 && tag[1] == "/" && tag.indexOf(" ") == -1 && tag[tag.length - 1] == ">") {
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
        if (tn == "br" || tn == "meta" || tn == "hr" || tn == "input" || tn == "link" || tn == "!doctype") {
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