i=0
while [ $i -ne 100 ]
do
        i=$(($i+1))
        echo Iteration $i
        ./work.sh
	sleep 30m
done
