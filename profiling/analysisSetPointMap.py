""" Python script for data analysis """
import numpy as np
import pandas as pd
import matplotlib
matplotlib.use("pgf")
matplotlib.rcParams["pgf.rcfonts"] = False
import matplotlib.pyplot as plt

from util import load_dataframe

def load_and_plot(num_wave_particles):
    df1 = load_dataframe("data/Set Point Map GPU {}.json".format(num_wave_particles))
    df2 = load_dataframe("data/Set Point Map CPU {}.json".format(num_wave_particles))

    df1.FunctionName = df1.FunctionName.apply(lambda x: x + " GPU")
    df2.FunctionName = df2.FunctionName.apply(lambda x: x + " CPU")
    df1["Implementation"] = "GPU"
    df2["Implementation"] = "CPU"

    df = pd.concat([df1, df2])
    df["NumParticles"] = num_wave_particles
    
    ######################
    ### Plotting Time! ###
    ######################
    matplotlib.style.use('ggplot')

    plt.figure()
    ax = df.loc[df.Implementation == "CPU"].TotalTime.plot(
        label="Set Point Map CPU, mean frame time = {:.3g}ms".format(df.loc[df.Implementation == "CPU"].TotalTime.mean())
    )

    ax = df.loc[df.Implementation == "GPU"].TotalTime.plot(
        ax = ax,
        label="Set Point Map GPU, mean frame time = {:.3g}ms".format(df.loc[df.Implementation == "GPU"].TotalTime.mean())
    )

    ax.set_title("Time taken per frame by Set Point Map function with {} Wave Particles".format(num_wave_particles))
    ax.set_xlabel("Frame Number")
    ax.set_ylabel("Frame Time (ms)")

    # Plots for garbage collection (not sure if needed)

    # plt.subplot(2, 1, 2)

    # ax = df.loc[df.Implementation == "CPU"].GCMemoryCumsum.plot(
    #     label="Set Point Map CPU, mean garbage allocation = {:.3g}ms".format(df.loc[df.Implementation == "CPU"].GCMemoryCumsum.mean())
    # )

    # ax = df.loc[df.Implementation == "GPU"].GCMemoryCumsum.plot(
    #     ax = ax,
    #     label="Set Point Map GPU, mean garbage allocation = {:.3g}ms".format(df.loc[df.Implementation == "GPU"].GCMemoryCumsum.mean())
    # )

    # ax.set_title("Cumulative gargbage allocated per frame by Set Point Map function with {} Wave Particles".format(num_wave_particles))
    # ax.set_xlabel("Frame Number")
    # ax.set_ylabel("Cumulative Garbage Allocated")

    plt.legend()
    return df



if __name__ == "__main__":
    figure = 0
    plt_figure = lambda: plt.figure(figure + 1)

    num_wave_particles = [10000, 50000, 100000, 500000, 1000000, 5000000]
    dfs = []
    for num_wave_particle in num_wave_particles:
        plt_figure()
        dfs.append(load_and_plot(num_wave_particle))

    df = pd.concat(
        dfs
    )

    print(df)

    grouped = df.groupby(["NumParticles", "FunctionName"])

    plt_figure()

    means = grouped.aggregate(np.mean)

    print means.unstack(level=1)
    ax = means.unstack(level=1).TotalTime.plot(marker='o', linestyle='--')
    ax.set_xlim([0, 5000000])
    vars = grouped.var()

    # plt.show()
    
    figs = [plt.figure(n) for n in plt.get_fignums()]
    figno = 0
    for fig in figs:
        fig.savefig("figure_{}.pdf".format(figno))
        fig.savefig("figure_{}.pgf".format(figno))
        figno = figno + 1

    # print("Data Amount: {}".format(df.shape))