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
    fig1, axe = plt.subplots(figsize=(30, 8))

    table = table[table['level'] == level]
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


if __name__ == '__main__':
    inputTables = Diversity.getTableFilesInFolder('./data/f65acba/')
    outputTable = runAnalysis(inputTables)
