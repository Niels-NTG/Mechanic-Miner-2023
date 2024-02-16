import glob

import pandas as pd
import matplotlib.pyplot as plt


def getTableFilesInFolder(path: str) -> pd.DataFrame:
    files = glob.glob(f'{path}GA log *.csv')
    frames = pd.concat([pd.read_csv(f) for f in files], ignore_index=True)
    # where filter causes filtered rows to be replaced with NaN
    return frames.where(frames['fitness'] > 0)


def runAnalysis(tables: pd.DataFrame):
    groupedData = tables.groupby(['level', 'generation'])
    uniqueGeneCounts = pd.DataFrame(
        columns=['level', 'generation', 'uniqueGenes']
    )
    for name, group in groupedData:
        tgmColumns = group[['gameObject', 'component', 'componentField', 'modifier']].dropna().drop_duplicates()
        newRow = pd.Series({
            'level': name[0],
            'generation': name[1],
            'uniqueGenes': tgmColumns.value_counts().size,
        })
        uniqueGeneCounts = pd.concat([uniqueGeneCounts, newRow.to_frame().T], ignore_index=True)

    fig, axes = plt.subplots(nrows=1, ncols=3, figsize=(12, 3))
    uniqueGeneCounts[uniqueGeneCounts['level'] == 3].plot(kind='line', y=['uniqueGenes'], x='generation', ax=axes[0])
    axes[0].set_title('Level 3')
    axes[0].set_xlim(1, 15)
    axes[0].set_ylim(0)
    uniqueGeneCounts[uniqueGeneCounts['level'] == 4].plot(kind='line', y=['uniqueGenes'], x='generation', ax=axes[1])
    axes[1].set_title('Level 4')
    axes[1].set_xlim(1, 15)
    axes[1].set_ylim(0)
    axes[1].get_legend().remove()
    uniqueGeneCounts[uniqueGeneCounts['level'] == 5].plot(kind='line', y=['uniqueGenes'], x='generation', ax=axes[2])
    axes[2].set_title('Level 5')
    axes[2].set_xlim(1, 15)
    axes[2].set_ylim(0)
    axes[2].get_legend().remove()


diversityTables = getTableFilesInFolder('./data/bb27b9d 10p elite/')
runAnalysis(diversityTables)

plt.tight_layout()
plt.show()
