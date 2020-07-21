import React from "react";
// import Chart from "react-google-charts";
const index = () => {
  const data = [
    ["Year", "Sales"],
    ["2004", 1000],
    ["2005", 1170],
    ["2006", 660],
    ["2007", 1030],
  ];
  const options = {
    title: "Company Performance",
    curveType: "function",
    legend: { position: "bottom" },
  };
  return (
    <div>
      Admin dashboard
      {/* <Chart chartType="LineChart" width="100%" height="400px" data={data} options={options} /> */}
    </div>
  );
};

export default index;
