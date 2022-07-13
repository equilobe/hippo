#!/usr/bin/python

import sys
from os import listdir, remove
from os.path import isfile, join

filesPath = '.openapi-generator/FILES'
apiFolder = 'api/'
modelFolder = 'model/'

apiRootPath = sys.argv[1]
file = open(apiRootPath + filesPath, "r")
lines = file.readlines()

apis = []
models = []

for line in lines:
    if "api/" in line:
        apis.append(line.split("/")[1].strip())
    if "model/" in line:
        models.append(line.split("/")[1].strip())

allApiFiles = [f for f in listdir(apiRootPath + apiFolder) if isfile(join(apiRootPath + apiFolder, f))]
allModelFiles = [f for f in listdir(apiRootPath + modelFolder) if isfile(join(apiRootPath + modelFolder, f))]

for file in allApiFiles:
    if file not in apis:
        remove(apiRootPath + apiFolder + file)

for file in allModelFiles:
    if file not in models:
        remove(apiRootPath + modelFolder + file)
