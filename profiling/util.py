import json

import pandas as pd

def load_dataframe(data_location):
    """
    This function extracts the data from Unity profiling as a pandas dataframe.
    """
    data = json.load(open(data_location))
    data = data["frames"]
    columns = []
    for values in data[0]["functions"][0]["values"]:
        columns.append(values["column"])
    reorg_data = []
    for frame in data:
        for function in frame["functions"]:
            row = []
            for value in function["values"]:
                row.append(str(value["value"]))
            reorg_data.append(row)
    df = pd.DataFrame(data=reorg_data, columns=columns)
    df.TotalTime = pd.to_numeric(df.TotalTime, errors="coerce")
    df.Calls = pd.to_numeric(df.Calls, errors="coerce")
    df.GCMemory = pd.to_numeric(df.GCMemory.apply(lambda x: x.strip(' B')), errors="coerce")
    df["GCMemoryCumsum"] = df.GCMemory.cumsum()
    df.TotalPercent = pd.to_numeric(df.TotalPercent.apply(lambda x: x.strip('%')), errors="coerce")
    df.SelfPercent = pd.to_numeric(df.SelfPercent.apply(lambda x: x.strip('%')), errors="coerce")
    return df