import glob

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import gower


def getTableFilesInFolder(path: str) -> pd.DataFrame:
    files = glob.glob(f'{path}GA log *.csv')
    frames = pd.concat([pd.read_csv(f) for f in files], ignore_index=True)
    # where filter causes filtered rows to be replaced with NaN
    return frames.where(frames['fitness'] > 1.e-10)
    # return frames


def runAnalysis(tables: pd.DataFrame):
    groupedData = tables.groupby(['level', 'generation'])
    gowerSums = pd.DataFrame(
        columns=['level', 'generation', 'gowerSum']
    )
    for name, group in groupedData:
        tgmColumns = group[['gameObject', 'component', 'componentField', 'modifier']]
        X = np.asarray(tgmColumns)
        gowerMatrix = gower.gower_matrix(X)
        gowerSum = np.sum(gowerMatrix.flatten())
        newRow = pd.Series({
            'level': name[0],
            'generation': name[1],
            'gowerSum': gowerSum,
        })
        gowerSums = pd.concat([gowerSums, newRow.to_frame().T], ignore_index=True)

    fig, axes = plt.subplots(nrows=1, ncols=3, figsize=(12, 3))
    gowerSums[gowerSums['level'] == 3].plot(kind='line', y=['gowerSum'], x='generation', ax=axes[0])
    axes[0].set_title('Level 3')
    axes[0].set_xlim(1, 15)
    gowerSums[gowerSums['level'] == 4].plot(kind='line', y=['gowerSum'], x='generation', ax=axes[1])
    axes[1].set_title('Level 4')
    axes[1].set_xlim(1, 15)
    axes[1].get_legend().remove()
    gowerSums[gowerSums['level'] == 5].plot(kind='line', y=['gowerSum'], x='generation', ax=axes[2])
    axes[2].set_title('Level 5')
    axes[2].set_xlim(1, 15)
    axes[2].get_legend().remove()


diversityTables = getTableFilesInFolder('./data/2p elite/')
runAnalysis(diversityTables)

plt.tight_layout()
plt.show()
