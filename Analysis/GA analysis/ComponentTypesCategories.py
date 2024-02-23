import glob

import pandas as pd
import matplotlib.pyplot as plt


def getTableFilesInFolder(path: str) -> pd.DataFrame:
    files = glob.glob(f'{path}GA log *.csv')
    frames: list[pd.DataFrame] = []
    for file in files:
        frame = pd.read_csv(file)
        # where filter causes filtered rows to be replaced with NaN
        frame = frame.where(frame['fitness'] > 0)
        frame['filename'] = file.split('/')[-1]
        frame['TGM'] = (
            frame['gameObject'].astype(str) + ',' +
            frame['component'].astype(str) + ',' +
            frame['componentField'].astype(str) + ',' +
            frame['modifier'].astype(str)
        )
        frames.append(frame)
    mergedFrames = pd.concat(frames, ignore_index=True)
    return mergedFrames


def runAnalysis(tables: pd.DataFrame):
    groupedData = tables.groupby(['level', 'generation'])
    allComponentTypes = tables.dropna().drop_duplicates(subset=['component'])['component'].to_list()
    populationDiversityTable = pd.DataFrame(
        columns=[
            'level',
            'generation',
            'totalUniqueGenes',
            *allComponentTypes
        ]
    )
    for name, group in groupedData:
        totalUniqueGeneCount = group['TGM'].agg(['nunique'])['nunique']
        uniqueComponentCount = group.drop_duplicates(subset=['TGM']).value_counts(subset=['component'])
        newRow = pd.Series({
            'level': name[0],
            'generation': name[1],
            'totalUniqueGenes': totalUniqueGeneCount,
        })
        for componentName, componentTypeCount in uniqueComponentCount.items():
            newRow[componentName] = componentTypeCount
        populationDiversityTable = pd.concat([populationDiversityTable, newRow.to_frame().T], ignore_index=True)

    fig, axes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makePlot(3, 'Wall', populationDiversityTable, allComponentTypes, 0, 0, axes)
    makePlot(4, 'Wall + Elevation', populationDiversityTable, allComponentTypes, 1, 0, axes)
    makePlot(5, 'Ceiling', populationDiversityTable, allComponentTypes, 2, 0, axes)
    makePlot(6, 'Deadly River', populationDiversityTable, allComponentTypes, 0, 1, axes)
    makePlot(8, 'Ravine', populationDiversityTable, allComponentTypes, 1, 1, axes)
    makePlot(9, 'Ravine + Spikes', populationDiversityTable, allComponentTypes, 2, 1, axes)


def makePlot(level: int, levelName: str, table: pd.DataFrame, componentTypes: list, x: int, y: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='area',
        y=componentTypes,
        x='generation',
        ax=axes[y, x],
    )
    plot.set_title(levelName)
    plot.set_xlim(1, 15)
    plot.set_ylim(0, 50)
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')
    if x != 2 or y != 0:
        plot.get_legend().remove()
    else:
        axes[y, x].legend(loc='upper left', bbox_to_anchor=(1.01, 1))


diversityTables = getTableFilesInFolder('./data/f65acba/')
runAnalysis(diversityTables)

plt.tight_layout()
plt.show()
