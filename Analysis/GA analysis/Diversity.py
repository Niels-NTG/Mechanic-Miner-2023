import glob

import pandas as pd
import matplotlib.pyplot as plt


levels: dict[int, str] = {
    3: '(A) "Wall"',
    4: '(B) "Wall + Elevation"',
    5: '(C) "Ceiling"',
    6: '(D) "Deadly River"',
    8: '(E) "Ravine"',
    9: '(F) "Ravine + Spikes"',
}


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


def getTableFilesInFolder(path: str, includeZeroFitness: bool = False) -> pd.DataFrame:
    files = glob.glob(f'{path}GA log *.csv')
    frames: list[pd.DataFrame] = []
    for file in files:
        frame = pd.read_csv(file)
        # where filter causes filtered rows to be replaced with NaN
        if not includeZeroFitness:
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
    return mergedFrames.dropna(subset=['fitness'])


def runAnalysis(tables: pd.DataFrame):
    groupedData = tables.groupby(['level', 'generation'])
    tgmGroups = tables.drop_duplicates(subset=['TGMgroup'])['TGMgroup'].to_list()
    tgmGroups.sort()
    populationDiversityTable = pd.DataFrame(
        columns=[
            'level',
            'generation',
            'median unique genes count',
            'unique genes count 5%',
            'unique genes count 25%',
            'unique genes count 75%',
            'unique genes count 95%',
            'median non-zero fitness population size',
            'population size 5%',
            'population size 25%',
            'population size 75%',
            'population size 95%',
            # 'totalUniqueGenes',
            'fitness median',
            'fitness 5%',
            'fitness 25%',
            'fitness 75%',
            'fitness 95%',
        ]
    )
    medianTGMCountTable = pd.DataFrame(
        columns=[
            'level',
            'generation',
            'median',
            '%',
            '5%',
            '25%',
            '75%',
            '95%',
            'min',
            'max',
        ]
    )

    medianTGMGroupCountTable = pd.DataFrame(
        columns=[
            'level',
            'generation',
            'median',
            '%',
            '5%',
            '25%',
            '75%',
            '95%',
            'min',
            'max',
        ]
    )

    for name, group in groupedData:
        # totalUniqueGeneCount = group['TGM'].agg(['nunique'])['nunique']
        uniqueGeneCount = group.groupby(['filename'])['TGM'].agg(['nunique'])
        uniqueGeneCountPercentiles = uniqueGeneCount.describe(
            percentiles=[0.05, 0.25, 0.75, 0.95]
        )
        uniqueGeneCountMedian = uniqueGeneCount.agg(['median'])

        populationCount = group.groupby(['filename'])['TGM'].agg(['count'])
        populationCountPercentiles = populationCount.describe(
            percentiles=[0.05, 0.25, 0.75, 0.95]
        )
        populationCountMedian = populationCount.agg(['median'])

        fitnessMedians = group.groupby(['filename'])['fitness'].agg(['median'])
        fitnessPercentiles = fitnessMedians.describe(
            percentiles=[0.05, 0.25, 0.75, 0.95]
        )
        fitnessMedian = fitnessMedians.agg(['median'])

        newRow = pd.Series({
            'level': name[0],
            'generation': name[1],
            'median unique genes count': uniqueGeneCountMedian['nunique']['median'],
            'unique genes count 5%': uniqueGeneCountPercentiles['nunique']['5%'],
            'unique genes count 25%': uniqueGeneCountPercentiles['nunique']['25%'],
            'unique genes count 75%': uniqueGeneCountPercentiles['nunique']['75%'],
            'unique genes count 95%': uniqueGeneCountPercentiles['nunique']['95%'],
            'median non-zero fitness population size': populationCountMedian['count']['median'],
            'population size 5%': populationCountPercentiles['count']['5%'],
            'population size 25%': populationCountPercentiles['count']['25%'],
            'population size 75%': populationCountPercentiles['count']['75%'],
            'population size 95%': populationCountPercentiles['count']['95%'],
            # 'totalUniqueGenes': totalUniqueGeneCount,
            'fitness median': fitnessMedian['median']['median'],
            'fitness 5%': fitnessPercentiles['median']['5%'],
            'fitness 25%': fitnessPercentiles['median']['25%'],
            'fitness 75%': fitnessPercentiles['median']['75%'],
            'fitness 95%': fitnessPercentiles['median']['95%'],
        })
        populationDiversityTable = pd.concat([populationDiversityTable, newRow.to_frame().T], ignore_index=True)

        medianTGMCountSubTable = group.groupby(['TGM'])['filename'].value_counts().groupby(['TGM']).agg([
            'median',
            ('5%', lambda x: x.quantile(0.05)),
            ('25%', lambda x: x.quantile(0.25)),
            ('75%', lambda x: x.quantile(0.75)),
            ('95%', lambda x: x.quantile(0.95)),
            'min',
            'max',
        ])
        medianTGMCountSubTable['%'] = medianTGMCountSubTable['median'].divide(medianTGMCountSubTable['median'].sum())
        medianTGMCountSubTable['level'] = name[0]
        medianTGMCountSubTable['generation'] = name[1]
        medianTGMCountTable = pd.concat([medianTGMCountTable, medianTGMCountSubTable])

        medianTGMGroupCountSubTable = group.groupby(['TGMgroup'])['filename'].value_counts().groupby(['TGMgroup']).agg([
            'median',
            ('5%', lambda x: x.quantile(0.05)),
            ('25%', lambda x: x.quantile(0.25)),
            ('75%', lambda x: x.quantile(0.75)),
            ('95%', lambda x: x.quantile(0.95)),
            'min',
            'max',
        ])
        medianTGMGroupCountSubTable['%'] = medianTGMGroupCountSubTable['median'].divide(medianTGMGroupCountSubTable['median'].sum())
        medianTGMGroupCountSubTable['level'] = name[0]
        medianTGMGroupCountSubTable['generation'] = name[1]
        medianTGMGroupCountTable = pd.concat([medianTGMGroupCountTable, medianTGMGroupCountSubTable])

    medianTGMCountTable.to_csv('./data/TGM median f9f6c53 40.csv')
    medianTGMGroupCountTable.to_csv('./data/TGM median group types f9f6c53 40.csv')
    populationDiversityTable.to_csv('./data/TGM diversity f9f6c53 40.csv')

    fig1, medianFitnessAxes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeMedianFitnessPlot(3, populationDiversityTable, 0, 0, medianFitnessAxes)
    makeMedianFitnessPlot(4, populationDiversityTable, 1, 0, medianFitnessAxes)
    makeMedianFitnessPlot(5, populationDiversityTable, 2, 0, medianFitnessAxes)
    makeMedianFitnessPlot(6, populationDiversityTable, 0, 1, medianFitnessAxes)
    makeMedianFitnessPlot(8, populationDiversityTable, 1, 1, medianFitnessAxes)
    makeMedianFitnessPlot(9, populationDiversityTable, 2, 1, medianFitnessAxes)
    plt.tight_layout()
    plt.show()
    fig1.savefig('./plots/median fitness level 3-4-5-6-8-9 f9f6c53 40.png')

    fig2, medianUniqueGeneCountAxes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeMedianUniqueGeneCountPlot(3, populationDiversityTable, 0, 0, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(4, populationDiversityTable, 1, 0, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(5, populationDiversityTable, 2, 0, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(6, populationDiversityTable, 0, 1, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(8, populationDiversityTable, 1, 1, medianUniqueGeneCountAxes)
    makeMedianUniqueGeneCountPlot(9, populationDiversityTable, 2, 1, medianUniqueGeneCountAxes)
    plt.tight_layout()
    plt.show()
    fig2.savefig('./plots/median unique genes count level 3-4-5-6-8-9 f9f6c53 40.png')

    fig3, medianNonZeroFitnessPopulationCountAxes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeMedianNonZeroFitnessPopulationCountPlot(3, populationDiversityTable, 0, 0, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCountPlot(4, populationDiversityTable, 1, 0, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCountPlot(5, populationDiversityTable, 2, 0, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCountPlot(6, populationDiversityTable, 0, 1, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCountPlot(8, populationDiversityTable, 1, 1, medianNonZeroFitnessPopulationCountAxes)
    makeMedianNonZeroFitnessPopulationCountPlot(9, populationDiversityTable, 2, 1, medianNonZeroFitnessPopulationCountAxes)
    plt.tight_layout()
    plt.show()
    fig3.savefig('./plots/median non-zero fitness population count level 3-4-5-6-8-9 f9f6c53 40.png')

    fig4, tgmCategoriesAxes = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeTGMCategoriesPlot(3, medianTGMGroupCountTable, tgmGroups, 0, 0, tgmCategoriesAxes)
    makeTGMCategoriesPlot(4, medianTGMGroupCountTable, tgmGroups, 1, 0, tgmCategoriesAxes)
    makeTGMCategoriesPlot(5, medianTGMGroupCountTable, tgmGroups, 2, 0, tgmCategoriesAxes)
    makeTGMCategoriesPlot(6, medianTGMGroupCountTable, tgmGroups, 0, 1, tgmCategoriesAxes)
    makeTGMCategoriesPlot(8, medianTGMGroupCountTable, tgmGroups, 1, 1, tgmCategoriesAxes)
    makeTGMCategoriesPlot(9, medianTGMGroupCountTable, tgmGroups, 2, 1, tgmCategoriesAxes)
    plt.tight_layout()
    plt.show()
    fig4.savefig('./plots/TGM groups level 3-4-5-6-8-9 f9f6c53 40.png')

    fig5, tgmCategoriesAxes2 = plt.subplots(nrows=2, ncols=3, figsize=(18, 8))
    makeTGMCategoriesAbsolutePlot(3, medianTGMGroupCountTable, tgmGroups, 0, 0, tgmCategoriesAxes2)
    makeTGMCategoriesAbsolutePlot(4, medianTGMGroupCountTable, tgmGroups, 1, 0, tgmCategoriesAxes2)
    makeTGMCategoriesAbsolutePlot(5, medianTGMGroupCountTable, tgmGroups, 2, 0, tgmCategoriesAxes2)
    makeTGMCategoriesAbsolutePlot(6, medianTGMGroupCountTable, tgmGroups, 0, 1, tgmCategoriesAxes2)
    makeTGMCategoriesAbsolutePlot(8, medianTGMGroupCountTable, tgmGroups, 1, 1, tgmCategoriesAxes2)
    makeTGMCategoriesAbsolutePlot(9, medianTGMGroupCountTable, tgmGroups, 2, 1, tgmCategoriesAxes2)
    plt.tight_layout()
    plt.show()
    fig5.savefig('./plots/TGM groups absolute level 3-4-5-6-8-9 f9f6c53 40.png')


def makeMedianFitnessPlot(level: int, table: pd.DataFrame, x: int, y: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='line',
        y=['fitness median'],
        x='generation',
        ax=axes[y, x],
        color='darkorange',
    )
    plot.fill_between(
        table['generation'],
        table['fitness 5%'],
        table['fitness 95%'],
        alpha=0.2,
        color='darkorange',
    )
    plot.fill_between(
        table['generation'],
        table['fitness 25%'],
        table['fitness 75%'],
        alpha=0.4,
        color='darkorange',
    )
    plot.set_title(levels.get(level))
    plot.set_xlim(1, 15)
    plot.set_ylim(0, 1)
    plot.set_xlabel('')
    if x != 0 or y != 0:
        plot.get_legend().remove()
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')


def makeMedianUniqueGeneCountPlot(level: int, table: pd.DataFrame, x: int, y: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='line',
        y=['median unique genes count'],
        x='generation',
        ax=axes[y, x],
        color='g',
    )
    plot.fill_between(
        table['generation'],
        table['unique genes count 5%'],
        table['unique genes count 95%'],
        alpha=0.2,
        color='g',
    )
    plot.fill_between(
        table['generation'],
        table['unique genes count 25%'],
        table['unique genes count 75%'],
        alpha=0.4,
        color='g',
    )
    plot.set_title(levels.get(level))
    plot.set_xlim(1, 15)
    plot.set_ylim(1, 14)
    plot.set_xlabel('')
    if x != 0 or y != 0:
        plot.get_legend().remove()
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')


def makeMedianNonZeroFitnessPopulationCountPlot(level: int, table: pd.DataFrame, x: int, y: int, axes):
    table = table[table['level'] == level]
    plot = table.plot(
        kind='line',
        y=['median non-zero fitness population size'],
        x='generation',
        ax=axes[y, x],
        color='purple',
    )
    plot.fill_between(
        table['generation'],
        table['population size 5%'],
        table['population size 95%'],
        alpha=0.2,
        color='purple',
    )
    plot.fill_between(
        table['generation'],
        table['population size 25%'],
        table['population size 75%'],
        alpha=0.4,
        color='purple',
    )
    plot.set_title(levels.get(level))
    plot.set_xlim(1, 15)
    plot.set_ylim(0, 100)
    if x != 0 or y != 0:
        plot.get_legend().remove()
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')


def makeTGMCategoriesPlot(level: int, table: pd.DataFrame, tgmTypes: list, x: int, y: int, axes):
    table = table[table['level'] == level]
    table1 = table[['generation', '%']].pivot(columns=['generation']).transpose().droplevel(0)
    table2 = pd.DataFrame(
        columns=tgmTypes
    )
    table2 = pd.concat([table2, table1])
    table2.to_csv('./data/median TGM types level {0} f9f6c53 40.csv'.format(level), index=False)
    plot = table2.plot(
        kind='area',
        y=tgmTypes,
        ax=axes[y, x],
        colormap='tab10',
        linewidth=0,
    )
    plot.set_title(levels.get(level))
    plot.set_xlim(1, 15)
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')
    if x != 2 or y != 0:
        plot.get_legend().remove()
    else:
        axes[y, x].legend(loc='upper left', bbox_to_anchor=(1.01, 1))


def makeTGMCategoriesAbsolutePlot(level: int, table: pd.DataFrame, tgmTypes: list, x: int, y: int, axes):
    table = table[table['level'] == level]
    medianTable1 = table[['generation', 'median']].pivot(columns=['generation']).transpose().droplevel(0)
    medianTable2 = pd.DataFrame(
        columns=tgmTypes
    )
    medianTable2 = pd.concat([medianTable2, medianTable1])
    plot = medianTable2.plot(
        kind='area',
        y=tgmTypes,
        ax=axes[y, x],
        colormap='tab10',
        linewidth=0,
    )
    plot.set_title(levels.get(level))
    plot.set_xlim(1, 15)
    if y == 1:
        plot.set_xlabel('generation')
    else:
        plot.set_xlabel('')
    if x != 2 or y != 0:
        plot.get_legend().remove()
    else:
        axes[y, x].legend(loc='upper left', bbox_to_anchor=(1.01, 1))


if __name__ == "__main__":
    diversityTables = getTableFilesInFolder('./data/f9f6c53/', False)
    runAnalysis(diversityTables)
