class SonarQubeTree
{
    public SonarQubeComponent baseComponent;
    public SonarQubeComponent[] components;
    public SonarQubePaging paging;
}

class SonarQubeMeasureResponse
{
    public SonarQubeComponent baseComponent;
    public SonarQubeComponent[] components;
}

class SonarQubeComponent
{
    public string id;
    public string key;
    public string language;
    public SonarQubeMeasure[] measures;
    public string name;
    public string path;
    public SonarQubeQualifier qualifier;
}

class SonarQubeMeasure
{
    public string metric;
    public float value;
    public SonarQubePeriod[] periods;
}

class SonarQubePeriod
{
    public float index;
    public string value;
}

class SonarQubePaging
{
    public int pageIndex;
    public int pageSize;
    public int total;
}

enum SonarQubeQualifier
{
    BRC, DIR, FIL, TRK, UTS
}