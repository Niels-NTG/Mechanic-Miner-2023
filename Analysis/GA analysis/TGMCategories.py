import glob

import pandas as pd
import matplotlib.pyplot as plt


# gameObject:
# - player = PlayerAgent
# - level = everything else
# component:
# - BoxCollider2D, CompositeCollider2D, EdgeCollider2D, TilemapCollider2D
# - Grid
# - PlayerController
# - RigidBody2D
# - Transform
def getComponentType(component: str):
    if 'Collider' in component:
        return 'collider'
    if 'Rigidbody' in component:
        return 'rigidbody'
    if 'Transform' in component:
        return 'transform'
    if 'Grid' in component:
        return 'grid'
    return 'other'


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
        frame['gameObjectType'] = [
            'player' if gameObject.startswith('Player') else 'level' for gameObject in frame['gameObject'].astype(str)
        ]
        frame['componentType'] = [
            getComponentType(component) for component in frame['component'].astype(str)
        ]
        frame['TGMgroup'] = frame['gameObjectType'].astype(str) + '-' + frame['componentType'].astype(str)

        frames.append(frame)
    mergedFrames = pd.concat(frames, ignore_index=True)
    return mergedFrames


def runAnalysis(tables: pd.DataFrame):
    groupedData = tables.groupby(['level', 'generation'])
    tgmGroups = tables.dropna().drop_duplicates(subset=['TGMgroup'])['TGMgroup'].to_list()
    tgmGroups.sort()
    populationDiversityTable = pd.DataFrame(
        columns=[
            'level',
            'generation',
            *tgmGroups
        ]
    )
    for name, group in groupedData:
        newRow = pd.Series({
            'level': name[0],
            'generation': name[1],
        })
        tgmGroupCounts = group.groupby(['filename']).value_counts(
            subset=['TGMgroup']
        ).groupby(['TGMgroup']).agg(['median'])
        for tgmGroupName, r in tgmGroupCounts.iterrows():
            newRow[tgmGroupName] = r['median']
        populationDiversityTable = pd.concat([populationDiversityTable, newRow.to_frame().T], ignore_index=True)

    fig, axes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makePlot(3, 'Wall', populationDiversityTable, tgmGroups, 0, 0, axes)
    makePlot(4, 'Wall + Elevation', populationDiversityTable, tgmGroups, 1, 0, axes)
    makePlot(5, 'Ceiling', populationDiversityTable, tgmGroups, 2, 0, axes)
    makePlot(6, 'Deadly River', populationDiversityTable, tgmGroups, 0, 1, axes)
    makePlot(8, 'Ravine', populationDiversityTable, tgmGroups, 1, 1, axes)
    makePlot(9, 'Ravine + Spikes', populationDiversityTable, tgmGroups, 2, 1, axes)


def makePlot(level: int, levelName: str, table: pd.DataFrame, tgmTypes: list, x: int, y: int, axes):
    table = table[table['level'] == level]
    table[tgmTypes] = table[tgmTypes].divide(table[tgmTypes].sum(axis=1), axis=0)
    table.to_csv('./data/TGM types level {0} f65acba 40.csv'.format(level), index=False)
    plot = table.plot(
        kind='area',
        x='generation',
        y=tgmTypes,
        ax=axes[y, x],
        colormap='tab20b',
        linewidth=0,
    )
    plot.set_title(levelName)
    plot.set_xlim(1, 15)
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
