# ---
# jupyter:
#   jupytext:
#     formats: ipynb,py:light
#     text_representation:
#       extension: .py
#       format_name: light
#       format_version: '1.5'
#       jupytext_version: 1.15.0
#   kernelspec:
#     display_name: Python 3 (ipykernel)
#     language: python
#     name: python3
# ---

import os
import datetime as dt
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.dates as mdates

# Path for current Tests

path = os.getcwd()[:-11] +"\\TestComponents\\TestSets\\Residues"

# Get names and results from each test

tests = []
test_name = []
for file in os.listdir(path+"\\Outputs"):
    if file.endswith('.csv'):
        tests.append(file)       
        test_name.append(os.path.splitext(file)[0])

# Pack tests up into dataframe for graphing

# +
Alltests =[]
for t in tests[:]:  
    testframe = pd.read_csv(path+"\\Outputs\\"+t,index_col=0,dayfirst=True,date_format='%d/%m/%Y %H:%M:%S %p')  
    Alltests.append(testframe)   

AllData = pd.concat(Alltests,axis=1,keys=test_name)
AllData.sort_index(axis=0,inplace=True)
AllData.index = pd.to_datetime(AllData.index)
# -

# Make graph

# +
cols = ['k','k','b','b','g','g','y','y','r','r']
lines = ['-','--','-','--','-','--','-','--','-','--',]

Graph = plt.figure()
ax = Graph.add_subplot(1,1,1)
pos = 0
for t in test_name:
    plt.plot(AllData.loc[:,(t,'ResidueN')].cumsum(),lines[pos],color=cols[pos],label = t)
    pos +=1
plt.legend(loc=(1.1,0.2))
plt.ylabel('Cum Net Residue mineralisation (kg/ha)')
plt.xticks(rotation=60)
ax.xaxis.set_major_formatter(mdates.DateFormatter('%#d-%b'))
plt.savefig(path+'\\TimeCourse.png')
