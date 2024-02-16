import glob

import pandas as pd
import matplotlib.pyplot as plt


def getTableFilesInFolder(path: str, category: str) -> pd.DataFrame:
    files = glob.glob(f'{path}GA log *.csv')
    frames = pd.concat([pd.read_csv(f) for f in files], ignore_index=True)
    frames['category'] = category
    return frames.where(frames['fitness'] > 0)


def runAnalysis(tables: pd.DataFrame):
    groupedData = tables.groupby(['category', 'level', 'generation'])['fitness'].agg(['mean', 'std']).reset_index()

    fig, axes = plt.subplots(nrows=1, ncols=3, figsize=(12, 3))

    for name, group in groupedData[groupedData['level'] == 3].groupby('category'):
        plot = group.plot(kind='line', y=['mean'], x='generation', ax=axes[0], label=[name])
        plot.fill_between(group['generation'], group['mean'] - group['std'], group['mean'] + group['std'], alpha=0.2)
    axes[0].set_title('Level 3')
    axes[0].set_ylim(0, 1)
    axes[0].set_xlim(1, 15)
    for name, group in groupedData[groupedData['level'] == 4].groupby('category'):
        plot = group.plot(kind='line', y=['mean'], x='generation', ax=axes[1], label=[name])
        plot.fill_between(group['generation'], group['mean'] - group['std'], group['mean'] + group['std'], alpha=0.2)
    axes[1].set_title('Level 4')
    axes[1].set_ylim(0, 1)
    axes[1].set_xlim(1, 15)
    axes[1].get_legend().remove()
    for name, group in groupedData[groupedData['level'] == 5].groupby('category'):
        plot = group.plot(kind='line', y=['mean'], x='generation', ax=axes[2], label=[name])
        plot.fill_between(group['generation'], group['mean'] - group['std'], group['mean'] + group['std'], alpha=0.2)
    axes[2].set_title('Level 5')
    axes[2].set_ylim(0, 1)
    axes[2].set_xlim(1, 15)
    axes[2].get_legend().remove()


eliteSelectionComparisonTables = pd.concat([
    getTableFilesInFolder('./data/bb27b9d 0p elite/', '0% elite selection'),
    getTableFilesInFolder('./data/bb27b9d 2p elite/', '2% elite selection'),
    getTableFilesInFolder('./data/bb27b9d 10p elite/', '10% elite selection'),
    getTableFilesInFolder('./data/bb27b9d 25p elite/', '25% elite selection'),
], ignore_index=True)
runAnalysis(eliteSelectionComparisonTables)

plt.tight_layout()
plt.show()
