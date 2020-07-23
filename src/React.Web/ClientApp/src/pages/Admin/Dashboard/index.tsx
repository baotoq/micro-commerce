import React, { useEffect, useState } from "react";
import Chart from "react-google-charts";
import StatisticService from "../../../services/statistic-service";

const Index = () => {
  const [data, setData] = useState([["Month", "Order"]]);
  const [data2, setData2] = useState([["Month", "Review"]]);
  useEffect(() => {
    const fetchData = async () => {
      const orders = await StatisticService.getOrdersAsync();
      setData([["Month", "Order"], ...orders]);
      const reviews = await StatisticService.getReviewsAsync();
      setData2([["Month", "Review"], ...reviews]);
    };
    fetchData();
  }, []);

  return (
    <div>
      <Chart
        chartType="LineChart"
        width="100%"
        height="400px"
        data={data}
        options={{
          title: "Month Orders",
          curveType: "function",
          legend: { position: "bottom" },
        }}
      />
      <Chart
        chartType="LineChart"
        width="100%"
        height="400px"
        data={data2}
        options={{
          title: "Month Reviews",
          curveType: "function",
          legend: { position: "bottom" },
        }}
      />
    </div>
  );
};

export default Index;
