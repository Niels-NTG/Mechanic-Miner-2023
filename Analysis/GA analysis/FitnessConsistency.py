import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

import Diversity


def runAnalysis(table: pd.DataFrame):
    table = table.dropna(subset=['fitness'])

    makePlot(3, table)
    makePlot(4, table)
    makePlot(5, table)
    makePlot(6, table)
    makePlot(8, table)
    makePlot(9, table)

    return table


def makePlot(level: int, table: pd.DataFrame):
    table = table[table['level'] == level]
    groupedData = table.groupby(['TGM'])
    for name, group in groupedData:
        if len(group[group['fitness'] == 0]) == len(group):
            table = table.drop(group.index)

    fig1, axe = plt.subplots(figsize=(10, np.ceil(0.2 * len(table['TGM'].unique()))))

    table.plot.box(
        column='fitness',
        by='TGM',
        ax=axe,
        vert=False,
    )
    axe.set_title(Diversity.levels.get(level))
    axe.set_xlim(0, 1)
    axe.set_xlabel('Fitness per TGM')
    plt.tight_layout()
    plt.show()
    fig1.savefig(f'./plots/FitnessConsistency level {level} f9f6c53 40.png')


if __name__ == '__main__':
    inputTables = Diversity.getTableFilesInFolder('./data/f9f6c53/', True)
    outputTable = runAnalysis(inputTables)
