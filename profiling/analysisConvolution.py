""" Python script for data analysis """
import numpy as np
import pandas as pd
import matplotlib
matplotlib.use("pgf")
matplotlib.rcParams["pgf.rcfonts"] = False
import matplotlib.pyplot as plt

from util import load_dataframe

def load_and_plot(hardware, convolution_type, scale, resolution):
    df = load_dataframe("data/Convolution {} {} {} {}.json".format(hardware, convolution_type, scale, resolution))

    df.FunctionName = df.FunctionName.apply(lambda x: x + " {} {}".format( hardware, convolution_type))
    df["Hardware"] = hardware
    df["ConvolutionType"] = convolution_type
    df["Scale"] = scale
    df["Resolution"] = resolution

    return df



if __name__ == "__main__":
    matplotlib.style.use('ggplot')
    figure = 0
    plt_figure = lambda: plt.figure(figure + 1)

    hardwares = ["GPU"]
    convolution_types = ["2D"]
    scales = ["Scale", "NoScale"]
    resolutions = [10, 50, 100, 150, 190, 224]
    dfs = []
    for hardware in hardwares:
        for convolution_type in convolution_types:
            for scale in scales:
                for resolution in resolutions:
                    plt_figure()
                    dfs.append(load_and_plot(hardware, convolution_type, scale, resolution))

    df = pd.concat(
        dfs
    )

    grouped = df.groupby(["Hardware", "ConvolutionType", "Scale", "Resolution"])

    #plt_figure()

    means = grouped.aggregate(np.mean)

    ax = means.unstack(level=0).unstack(level=0).unstack(level=0).TotalTime.plot(marker='o', linestyle='--')
    ax.set_ylabel("Convolution Running Time (ms)")

    # plt.show()
    
    figs = [plt.figure(n) for n in plt.get_fignums()]
    figno = 0
    for fig in figs:
        fig.savefig("figure_{}.pdf".format(figno))
        fig.savefig("figure_{}.pgf".format(figno))
        figno = figno + 1
