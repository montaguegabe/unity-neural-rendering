
import os
import subprocess
import os.path
import random
import time
import string

START_IND = 30_000
NUM_HOUSES = 45_622

# My computer can only store images from around 500 houses at a time
AT_A_TIME = 500

SUNCG_BASE = '/Users/gabemontague/Courses/FinalProject/SunCG/'
APP = '/Users/gabemontague/Desktop/SUNCGGen.app'
DRIVE_BASE = '/Volumes/Drive/'

input('WARNING: The SUNCG/output folder is about to be wiped! Press Control-C now if you do not want this to happen!')

def call(cmd, silent=False):
    if not silent:
        print(cmd)
    result = os.system(cmd)
    assert(result == 0)

for batch_idx in range(START_IND, NUM_HOUSES, AT_A_TIME):

    # Move log to a new place
    random_str = ''.join(random.choice(string.ascii_uppercase + string.digits) for _ in range(8))
    if os.path.isfile(f'{SUNCG_BASE}completed.txt'):
        call(f'mkdir -p {SUNCG_BASE}record && mv {SUNCG_BASE}completed.txt {SUNCG_BASE}record/completed_{random_str}.txt')

    # CLEARS and remakes output directory
    call(f'rm -rf {SUNCG_BASE}output && mkdir {SUNCG_BASE}output')
    
    # Set start, end
    end = min(batch_idx + AT_A_TIME, NUM_HOUSES)
    call(f'echo "{batch_idx}\n{end}" > {SUNCG_BASE}range.txt')

    # Run the program. The "open" command is specific to OSX I believe so may need to be modified for Linux
    call(f'open {APP}')

    # Poll completion
    while not os.path.isfile(f'{SUNCG_BASE}completed.txt'):
        time.sleep(10)

    # Export results
    zip_name = f'batch_{batch_idx}.zip'
    dir_name = f'batch_{batch_idx}'
    call(f'cd {SUNCG_BASE} && mv output {dir_name} && zip -r {zip_name} {dir_name} && mv {zip_name} {DRIVE_BASE} && rm -rf {dir_name}')


    